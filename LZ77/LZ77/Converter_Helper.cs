using System.Collections;
using System.IO;
using System.Text;
using System.Drawing;

public static class Converter_Helper{

    public static string Txt_to_string(string path) {

        return File.ReadAllText(@path);
    }

    public static byte[] Img_to_byte(string path) {

        Image image = Image.FromFile(@path);

        using (var ms = new MemoryStream()) {

            image.Save(ms, image.RawFormat);

            return ms.ToArray();
        }
    }

    public static Image Bytes_to_img(byte[] bytes) {

        MemoryStream ms = new MemoryStream(bytes);
        Image image = Image.FromStream(ms);

        return image;
    }

    public static byte[] String_to_bytes(string text) {

        return Encoding.ASCII.GetBytes(text);
    }

    public static BitArray Bytes_to_binary(byte[] bytes) {

        return new BitArray(bytes);
    }

    public static byte[] Binary_to_byte(BitArray bits) {

        var bytes = new byte[(bits.Length - 1) / 8 + 1];
        bits.CopyTo(bytes, 0);

        return bytes;
    }

    public static string Bytes_to_string(byte[] bytes) {

        return System.Text.Encoding.ASCII.GetString(bytes);
    }

    public static void String_to_txt(string text, string path) {

        System.IO.File.WriteAllText(@path, text);
    }

    public static string Bit_to_string(BitArray bits, int sqtr_size) {

        string text = "";
        int counter_x = 0;
        int counter_y = 0;

        for (int i = 0; i < bits.Length; i++) {
     
            if (counter_x == sqtr_size) {
               
                counter_x = 0;
                counter_y++;

                if (counter_y == sqtr_size) {

                    return text;
                }

                text = text + System.Environment.NewLine;
            }

            if (bits[i] == true) {

                text = text + ".";
            }
            else {

                text = text + "X";
            }

            counter_x++;
        }

        return text;
    }

    public static void Bitmap_to_img(Bitmap bitmap, string path) {

        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
    }
}
