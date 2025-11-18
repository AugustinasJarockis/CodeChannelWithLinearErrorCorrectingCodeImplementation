namespace LinearCodeChannel.MathParts
{
    internal class Encoder
    {
        // Generuojanti matrica
        private readonly GeneratingMatrix _matrix;

        // Koduotojo sukūrimo metu nustatoma pradinė generuojančioji matrica
        public Encoder(GeneratingMatrix matrix) {
            _matrix = matrix;
        }

        // Žinutės užkodavimas naudojant generuojančiąją matricą
        // Parametrai: 
        // > message - masyvas, kurio kiekvienas elementas atitinka koduojamos žinutės bitą
        // Grąžinama reikšmė - masyvas, kurio kiekvienas elementas atitinka užkoduotos žinutės bitą
        public List<byte> Encode(List<byte> message) {
            // Užkoduotos žinutės masyvo sukūrimas su užkoduotos žinutės dydžio talpa
            byte[] encodedMessage = new byte[(int)(((message.Count + _matrix.Dimension - 1) / _matrix.Dimension) * _matrix.VectorSize!)];

            // Originalios žinutės papildymas nuliais, kad žinutę būtų galima suskaidyti reikiamo ilgio žodžiais
            while (message.Count % _matrix.Dimension != 0)
                message.Add(0);

            // Žinutės suskaidymas kodo žodžiais ir užkodavimas jais
            for (int i = 0; i < (message.Count + _matrix.Dimension - 1) / _matrix.Dimension; i++) {
                List<byte> vector = [..message.Skip(i * _matrix.Dimension).Take(_matrix.Dimension)];

                // Naudojant generuojančią matricą apskaičiuoti kiekvieną kodo žodžio vektoriaus narį
                for (int i2 = 0; i2 < _matrix.VectorSize; i2++) {
                    byte sum = 0;
                    for (int i3 = 0; i3 < vector.Count; i3++) {
                        sum = (byte)((sum + (vector[i3] * _matrix.Matrix[i3][i2])) % 2);
                    }
                    encodedMessage[(int)(i * _matrix.VectorSize! + i2)] = sum;
                }
            }

            return [.. encodedMessage];
        }
    }
}
