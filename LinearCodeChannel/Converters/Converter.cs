using System.Windows.Media.Imaging;

namespace LinearCodeChannel
{
    static class Converter {

        // Tekstinės eilutės konvertavimas pagal ASCII kodavimą į bitų masyvą
        // Parametrai: 
        // > str - tekstinė eilutė
        // Grąžinama reikšmė - masyvas, kurio kiekvienas elementas atitinka pateiktos tekstinės eilutės simbolio bitą
        public static List<byte> ToBits(this string str) {
            List<byte> bits = [];
            foreach (char c in str)
                AppendBits(bits, (byte)c);
            return bits;
        }

        // Bitų masyvo konvertavimas į tekstinę eilutę pagal ASCII koduotę
        // Parametrai: 
        // > bits - masyvas, kurio kiekvienas elementas atitinka tekstinės eilutės simbolio bitą
        // Grąžinama reikšmė - tekstinė eilutė, gauta konvertavus bitų masyvą į tekstą
        public static string ToCharString(this List<byte> bits) {
            string str = "";
            for (int i = 0; i < bits.Count / 8; i++) {
                byte symbol = 0;
                for (int i2 = 0; i2 < 8; i2++)
                    symbol = (byte)(symbol * 2 + bits[i * 8 + i2]);

                str += (char)symbol;
            }
            return str;
        }

        // Tekstinės eilutės, sudarytos iš 1 ir 0, kovertavimas į bitų masyvą
        // Parametrai: 
        // > str - tekstinė eilutė, sudaryta iš 1 ir 0
        // Grąžinama reikšmė - masyvas, kurio kiekvienas elementas atitinka pateiktos tekstinės eilutės simbolį, paverstą į skaičių
        public static List<byte> ToBitRepresentation(this string str) => [.. str.Where(c => c != ' ').Select(c => (byte)(c - '0'))];

        // Bitų masyvo konvertavimas yra tekstinę eilutę, kurioje vaizduojami tie bitai.
        // Kas 8 bitus padedamas tarpas
        // Parametrai: 
        // > bits - masyvas, kurio kiekvienas elementas atitinka bito reikšmę 1 arba 0
        // Grąžinama reikšmė - tekstinė eilutė, gauta kovertavus bitų masyvą, sudaryta tik iš 1 ir 0
        public static string ToStringRepresentation(this List<byte> bits) => 
            new ([.. bits.SelectMany(
                (b, i) => {
                    char c = (char) (b + '0');
                    if ((i + 1) % 8 == 0)
                        return c + " ";
                    return c.ToString();
                })]);

        // Funkcija, kuri prideda aštuonis bitus prie bitų masyvo pagal baito reikšmes
        // Parametrai: 
        // > bits - masyvas, kurio kiekvienas elementas atitinka bito reikšmę 1 arba 0 ir kurį norima papildyti
        // > value - baitas, kurio reikšmę norima pridėti į bitų masyvą
        private static void AppendBits(List<byte> bits, byte value) {
            bits.AddRange([
                (byte)((value & 0b10000000) != 0 ? 1 : 0),
                (byte)((value & 0b01000000) != 0 ? 1 : 0),
                (byte)((value & 0b00100000) != 0 ? 1 : 0),
                (byte)((value & 0b00010000) != 0 ? 1 : 0),
                (byte)((value & 0b00001000) != 0 ? 1 : 0),
                (byte)((value & 0b00000100) != 0 ? 1 : 0),
                (byte)((value & 0b00000010) != 0 ? 1 : 0),
                (byte)((value & 0b00000001) != 0 ? 1 : 0)
                ]);
        }

        // Funkcija, kuri pagal baito reikšmę nustato aštuonių bitų reikšmes bitų masyve
        // Parametrai: 
        // > bits - masyvas, kurio kiekvienas elementas atitinka bito reikšmę 1 arba 0 ir kurį norima papildyti
        // > value - baitas, kurio reikšmę norima pridėti į bitų masyvą
        // > index - indeksas, ties kuriuo masyve norima patalpinti reikšmę
        private static void SetBits(byte[] bits, byte value, int index) {
            bits[index + 0] = (byte)((value & 0b10000000) != 0 ? 1 : 0);
            bits[index + 1] = (byte)((value & 0b01000000) != 0 ? 1 : 0);
            bits[index + 2] = (byte)((value & 0b00100000) != 0 ? 1 : 0);
            bits[index + 3] = (byte)((value & 0b00010000) != 0 ? 1 : 0);
            bits[index + 4] = (byte)((value & 0b00001000) != 0 ? 1 : 0);
            bits[index + 5] = (byte)((value & 0b00000100) != 0 ? 1 : 0);
            bits[index + 6] = (byte)((value & 0b00000010) != 0 ? 1 : 0);
            bits[index + 7] = (byte)((value & 0b00000001) != 0 ? 1 : 0);
        }

        // Funkcija, kuri konvertuoja bitų masyvą į paveiksliuką. Paveiksliuko duomenys ir dimensijos nustatomos pagal nurodytą pavyzdinį paveiksliuką
        // Parametrai: 
        // > imageData - paveikslėlio duomenys išreikšti bitais
        // > imageMould - paveikslėlis, kuris naudojamas kaip pavyzdys nustatant konvertuoto paveikslėlio aukštį, plotį, koduotę bei paveikslėlio eilutės ilgį baitais
        // Grąžinama reikšmė - iš duomenų, pateiktų bitais, atkurtas paveikslėlis
        public static BitmapSource ToImage(this List<byte> imageData, BitmapSource imageMould) {
            byte[] imageBytes = new byte[imageData.Count/8];

            for (int i = 0; i < imageData.Count; i += 8) {
                imageBytes[i / 8] = (byte)(
                    imageData[i + 0] << 7 |
                    imageData[i + 1] << 6 |
                    imageData[i + 2] << 5 |
                    imageData[i + 3] << 4 |
                    imageData[i + 4] << 3 |
                    imageData[i + 5] << 2 |
                    imageData[i + 6] << 1 |
                    imageData[i + 7] << 0);
            }
            
            return BitmapSource.Create(
                imageMould.PixelWidth,
                imageMould.PixelHeight,
                96, 96,
                imageMould.Format,
                null,
                imageBytes,
                imageMould.PixelWidth * (imageMould.Format.BitsPerPixel / 8)
            );
        }

        // Funkcija, kuri konvertuoja paveiksliuko duomenis į bitų masyvą
        // Parametrai: 
        // > image - pavekslėlis
        // Grąžinama reikšmė - masyvas, kuriame užkoduoti paveiklėlio pikseliai bitais
        public static List<byte> ToBitArray(this BitmapSource image) {
            int bytesPerPixel = (image.Format.BitsPerPixel + 7) / 8;
            int stride = image.PixelWidth * bytesPerPixel;

            byte[] bytes = new byte[image.PixelHeight * stride];

            image.CopyPixels(bytes, stride, 0);

            byte[] bits = new byte[bytes.Length * 8];

            for (int i = 0; i < bytes.Length; i++)
                SetBits(bits, bytes[i], i * 8);

            return [.. bits];
        }
    }
}
