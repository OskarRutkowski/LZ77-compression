using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LZ77
{
    public partial class Form1 : Form
    {
        List<Pair> reader_list = new List<Pair>();
        OpenFileDialog ofd;

        byte[] inputFile; //tablica bajtów z plikiem wejściowym
        string inputText = "";
        int inputSize = 0; // wielkość tablicy bajtów z plikiem wejściowym
        string inputExt = ""; // rozszerzenie pliku wejściowego
        Dictionary<char, double> dictionary; // słownik używany przy liczeniu entropii

        public Dictionary<char, double> makeDictionary(string input)
        {
            string getMessage = input;
            int length = getMessage.Length;
            var counted = new Dictionary<char, int>();

            foreach (char value in getMessage)
            {
                if (counted.ContainsKey(value))
                    counted[value] = counted[value] + 1;
                else
                    counted.Add(value, 1);
            }
            var countedPercentage = new Dictionary<char, double>();
            var countedForEntrophy = new Dictionary<char, double>();
            foreach (KeyValuePair<char, int> entry in counted)
            {
                countedPercentage.Add(entry.Key, Math.Round(entry.Value / (double)length, 5) * 100);
                countedForEntrophy.Add(entry.Key, Math.Round(entry.Value / (double)length, 5));
            }
            countEntrophy(countedForEntrophy);
            label5.Text = length.ToString();
            label9.Text = length.ToString();
            label6.Text = counted.Count.ToString();
            label11.Text = counted.Count.ToString();
            return countedPercentage;
        }
        public void countEntrophy(Dictionary<char, double> dictionary)
        {
            double entrophy = 0;
            foreach (KeyValuePair<char, double> entry in dictionary)
            {
                entrophy += entry.Value * Math.Log(1 / entry.Value, 2);
            }
            entrophy = Math.Round(entrophy, 3);
            label13.Text = entrophy.ToString();

        }

        public Form1()
        {

            InitializeComponent();
            
        

            //czwarta zakładka
            textBox2.Text = "Lempel-Ziv 77, skracane zwykle do LZ77 (algorytm LZ77) – metoda strumieniowej słownikowej kompresji danych. Metoda LZ77 wykorzystuje fakt, że w danych powtarzają się ciągi bajtów (np. w tekstach naturalnych będą to słowa, frazy lub całe zdania) – kompresja polega na zastępowaniu powtórzonych ciągów o wiele krótszymi liczbami wskazującymi, kiedy wcześniej wystąpił ciąg i z ilu bajtów się składał; z punktu widzenia człowieka jest to informacja postaci \"taki sam ciąg o długości 15 znaków wystąpił 213 znaków wcześniej\". Algorytm LZ77 jest wolny od wszelkich patentów, co w dużej mierze przyczyniło się do jego popularności i szerokiego rozpowszechnienia. Doczekał się wielu ulepszeń i modyfikacji, dających lepsze współczynniki kompresji albo dużą szybkość działania.Na LZ77 opiera się m.in. algorytm deflate, używany jest również w formatach ZIP, gzip, ARJ, RAR, PKZIP, a także PNG. Algorytm został opracowany w 1977 przez Abrahama Lempela i Jacoba Ziv i opisany w artykule A universal algorithm for sequential data compression opublikowanym w IEEE Transactions on Information Theory(str. 8 - 19). Rok później autorzy opublikowali ulepszoną wersję metody, znaną pod nazwą LZ78. Organizacja IEEE uznała algorytm Lempel - Ziv za kamień milowy w rozwoju elektroniki i informatyki. ";
            textBox3.Text = "W LZ77 zapamiętywana jest w słowniku pewna liczba ostatnio kodowanych danych – przeciętnie kilka do kilkudziesięciu kilobajtów. Jeśli jakiś ciąg powtórzy się, to zostanie zastąpiony przez liczby określające jego pozycję w słowniku oraz długość ciągu; do zapamiętania tych dwóch liczb trzeba przeznaczyć zazwyczaj o wiele mniej bitów niż do zapamiętania zastępowanego ciągu. Metoda LZ77 zakłada, że ciągi powtarzają się w miarę często, tzn.na tyle często, żeby wcześniejsze wystąpienia można było zlokalizować w słowniku – ciągi powtarzające się zbyt rzadko nie są brane pod uwagę. Wady tej pozbawiona jest metoda LZ78, w której – przynajmniej teoretycznie – słownik rozszerza się w nieskończoność. Bardzo dużą zaletą kodowania LZ77(a także innych metod słownikowych z rodziny LZ, tj.LZSS, LZ78, LZW itp.) jest to, że słownika nie trzeba zapamiętywać i przesyłać wraz z komunikatem – zawartość słownika będzie na bieżąco odtwarzana przez dekoder. Algorytm kompresji jest bardziej złożony i trudniejszy w realizacji niż algorytm dekompresji. W metodzie LZ77 można wpływać na prędkość kompresji oraz zapotrzebowania pamięciowe, regulując parametry kodera(rozmiar słownika i bufora kodowania). ";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] buffer; //bufor do wczytania pliku z OFD
            ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                buffer = File.ReadAllBytes(ofd.FileName);
                inputText = File.ReadAllText(ofd.FileName);
                inputFile = buffer;
                inputSize = buffer.Length;
                inputExt = Path.GetExtension(ofd.FileName);
                string str = System.Text.Encoding.Default.GetString(buffer);
                dictionary = makeDictionary(str);

               
                //pierwsza zakładka
                label1.Text = ofd.SafeFileName;
                label5.Text = inputSize.ToString();
                //druga zakładka
                textBox1.Text = str;
                //trzecia zakładka
                dataGridView1.DataSource = (from data in dictionary orderby data.Value select new { data.Key, data.Value }).ToList();
            }
        }
        
        private void compress_Click(object sender, EventArgs e)
        {
            if(inputFile == null)
            {
                MessageBox.Show("Wprowadź plik do kompresji.", "Niewłaściwy plik", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                List<Pair> compressed;
                LZ77 lz = new LZ77();
                char[] textChar = inputText.ToCharArray();
                compressed = lz.Compress(inputText.ToCharArray());
                Console.WriteLine(compressed.Count);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "lz77 files (*.lz77)|*.lz77";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        using (BinaryWriter writer = new BinaryWriter(File.Open(sfd.FileName, FileMode.Create)))
                        {
                            foreach (Pair item in compressed)
                            {
                                writer.Write(item.lenght);
                                writer.Write(item.start_index);
                                writer.Write(item.letter);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception caught in process: {0}", ex);
                    }
                }
            }
            
            
        }

        private void decompress_Click(object sender, EventArgs e)
        {
            if (inputFile == null)
            {
                MessageBox.Show("Wprowadź plik do dekompresji.", "Niewłaściwy plik", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                LZ77 lzde = new LZ77();

                using (BinaryReader reader = new BinaryReader(File.Open(ofd.FileName, FileMode.Open)))
                {




                    // 2.
                    // Position and length variables.
                    int pos = 0;
                    // 2A.
                    // Use BaseStream.
                    int length = (int)reader.BaseStream.Length;

                    while (pos < length)
                    {
                        // 3.
                        // Read integer.
                        reader_list.Add(new Pair(reader.ReadByte(), reader.ReadByte(), reader.ReadChar()));

                        // 4.
                        // Advance our position variable.
                        pos += sizeof(byte);
                        pos += sizeof(byte);
                        pos += sizeof(char);

                    }

                    reader_list.Add(new Pair(reader.ReadByte(), reader.ReadByte(), reader.ReadChar()));

                }

                string d = lzde.Decompress(reader_list);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "lz77 files (*.lz77)|*.lz77|All files (*.*)|*.*|User input type (*" + inputExt + ")|*" + inputExt + "";
                if (inputExt != "")
                {
                    sfd.FilterIndex = 2;
                }

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, d);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception caught in process: {0}", ex);
                    }
                }
            }
            
            
        }
    }
}
