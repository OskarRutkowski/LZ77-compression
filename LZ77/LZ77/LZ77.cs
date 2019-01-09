using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Collections;

namespace LZ77
{
    class Matcher {

        public int lenght;
        public int start_index;

        public Matcher() {

            this.lenght = 1;
        }

        public Matcher(int start_index) {

            this.start_index = start_index;
            this.lenght = 1;
        }
    }

    class Pair {

        public byte lenght;
        public byte start_index;
        public char letter;

        public Pair(byte lenght, byte start_index, char letter) {

            this.lenght = lenght;
            this.start_index = start_index;
            this.letter = letter;
        }
    }

    class LZ77 {

        private List<Pair> output_list;

        public LZ77() {

            output_list = new List<Pair>();
        }

        public List<Pair> Compress(char[] data) {
            int window_length = 255;
            List<string> db = new List<string>();
            int array_lenght = data.Length;
            int look_ahead = 0;
            for (int i = 0; i < array_lenght; i++) {
                if (i != 0) {
                    look_ahead = window_length;
                    if (i - look_ahead < 0) 
                        look_ahead = i;
                    List <Matcher> match_list= new List<Matcher>();
                    int lctr = 0;
                    for (int j = i - 1; j > i - look_ahead - 1; j--) {
                        if (data[j] == data[i])
                            match_list.Add(new Matcher(lctr));
                        lctr++;
                    }
                    if (match_list.Count != 0) {
                        foreach (Matcher item in match_list) {
                            int ctr = 0;
                            for (int n = i - item.start_index; n < i; n++) {
                                ctr++;
                                if (i + ctr < data.Length) {
                                    if (data[n] == data[i + ctr]) 
                                        item.lenght++;
                                    else
                                        break;
                                }
                            }
                        }
                        Matcher result = new Matcher();
                        int counter = 0;
                        foreach (Matcher item in match_list) {
                            if (counter == 0) 
                                result = item;
                            else {
                                if (item.lenght > result.lenght) 
                                    result = item;
                            }
                            counter++;
                        }
                        if (i + result.lenght >= data.Length) {
                            db.Add("1" + result.start_index + result.lenght);
                            byte result_lenght = (byte)result.lenght;
                            byte result_start_index = (byte)result.start_index;
                            Add_pair(result_lenght, result_start_index, '.');
                        }
                        else {
                            db.Add("1" + data[i + result.lenght] + result.start_index +  "LLL" + result.lenght);
                            byte result_lenght = (byte)result.lenght;
                            byte result_start_index = (byte)result.start_index;
                            Add_pair(result_lenght, result_start_index, data[i + result.lenght]);
                        }
                        i = i + result.lenght;
                    }
                    else {
                        db.Add("0" + data[i]);
                        Add_pair(0, 0, data[i]);
                    }
                }
                else {
                    db.Add("0" + data[i]);
                    Add_pair(0, 0, data[i]);
                }
            }
            return output_list;            
        }

        private void Add_pair(byte lenght, byte start_index, char letter) {

            output_list.Add(new Pair(lenght, start_index, letter));
        }

        public string Decompress(List<Pair> input_list) {
            List<Char> decoded = new List<char>();
            int counter = 0;
            foreach (Pair item in input_list) {
                if (item.lenght == 0) {
                    decoded.Add(item.letter);
                    counter++;
                }
                else {
                    int counter_temp = counter;
                    for (int i = counter_temp - item.start_index - 1; i < counter_temp - item.start_index + item.lenght - 1; i++) {
                        decoded.Add(decoded[i]);
                        counter++;
                    }
                    decoded.Add(item.letter);
                    counter++;
                }
            }
            return new string(decoded.ToArray());
        }
    }
}   
