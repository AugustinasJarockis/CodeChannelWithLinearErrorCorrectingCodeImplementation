namespace LinearCodeChannel.MathParts
{
    internal class Decoder
    {
        // Generuojanti matrica
        private readonly GeneratingMatrix _matrix;

        // Koduotojo sukūrimo metu nustatoma pradinė generuojančioji matrica
        public Decoder(GeneratingMatrix matrix) {
            _matrix = matrix;
        }

        // Funkcija, atsakinga už žinutės, praėjusios per iškraipantį kanalą, dekodavimas
        // Parametrai: 
        // > message - masyvas, kurio kiekvienas elementas atitinka užkoduotos žinutės, kurioje gali būti klaidų, bitą
        // > originalSize - originalus žinutės dydis prieš užkodavimą
        // Grąžinama reikšmė - masyvas, kurio kiekvienas elementas atitinka dekoduotos žinutės bitą
        public List<byte> Decode(List<byte> message, int originalSize) {
            var correctedMessage = CorrectMistakes(message);
            var decodedMessage = DecodeMessage(correctedMessage);
            return [.. decodedMessage.Take(originalSize)];
        }

        // Klaidų taisymas, kiekvienas užėjusios žinutės žodžiui pritaikant kodo žodį
        // Parametrai: 
        // > message - masyvas, kurio kiekvienas elementas atitinka užkoduotos žinutės bitą. Šiame masyve gali būti įvykę klaidų
        // Grąžinama reikšmė - masyvas, kurio kiekvienas elementas atitinka užkoduotos žinutės bitą. Šiame masyve gali būti tik kodo žodžiai
        private List<byte> CorrectMistakes(List<byte> message) {
            // Masyvas, saugantis išėjusios žinutės bitus
            List<byte> correctedMessage = new(message.Count);

            int vecSize = 0;
            if (_matrix.VectorSize != null)
                vecSize = _matrix.VectorSize ?? 0;

            // Kiekvienam išėjusiam žodžiui priskiriamas kodo žodis
            for (int i = 0; i < (message.Count + vecSize - 1) / vecSize; i++) {
                // Paimamas kodo žodžio ilgio simbolių masyvas
                List<byte> vector = [.. message.Skip(i * vecSize).Take(vecSize)];

                // Apskaičiuojamas sindromo svoris. Jei jis lygus nuliui, esamas žodis yra kodo žodis ir yra laikomas ištaisytu ir pridedamas prie ištaisytos žinutės 
                var previousSyndromeWeight = _matrix.GetCorrespondingCosetLeaderWeight(vector);
                if (previousSyndromeWeight == 0) {
                    correctedMessage.AddRange(vector);
                    continue;
                }

                // Einama per kiekvieną žodžio bitą, jis pakeičiamas, tikrinama ar tai sumažina sindromo svorį
                // Jei taip, tai tas bitas lieka pakeistas. Jei ne, jis atstatomas atgal
                for (int i2 = 0; i2 < vector.Count; i2++) {
                    vector[i2] = (byte)((vector[i2] + 1) % 2);
                    // Patikrinimas, ar sumažėja sindromo svoris pakeitus bitą
                    var newSyndromeWeight = _matrix.GetCorrespondingCosetLeaderWeight(vector);
                    if (newSyndromeWeight < previousSyndromeWeight) {
                        // Patikrinimas, ar svoris lygus 0. Sindromo svorio reikšmės išsaugojimas
                        if (newSyndromeWeight == 0) {
                            correctedMessage.AddRange(vector);
                            break;
                        }
                        previousSyndromeWeight = newSyndromeWeight;
                    }
                    else {
                        // Bito atstatymas atgal
                        vector[i2] = (byte)((vector[i2] + 1) % 2);
                    }
                }
            }

            return correctedMessage;
        }

        // Kodo žodžių dekodavimas suformuojant atkurtą originalią žinutę
        // Parametrai: 
        // > message - masyvas, kurio kiekvienas elementas atitinka užkoduotos žinutės bitą. Šiame masyve yra tik kodo žodžiai, visos klaidos jame laikomos ištaisytomis
        // Grąžinama reikšmė - masyvas, kurio kiekvienas elementas atitinka dekoduotos žinutės bitą
        private List<byte> DecodeMessage(List<byte> message) {

            List<byte> decodedMessage = new((int)(message.Count / _matrix.VectorSize! * _matrix.Dimension));

            for (int i = 0; i < (message.Count + _matrix.VectorSize - 1) / _matrix.VectorSize; i++) {
                decodedMessage.AddRange(message.Skip((_matrix.VectorSize ?? 0) * i).Take(_matrix.Dimension));
            }

            return decodedMessage;
        }
    }
}
