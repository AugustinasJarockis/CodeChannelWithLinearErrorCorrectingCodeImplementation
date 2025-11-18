namespace LinearCodeChannel.MathParts
{
    internal class GeneratingMatrix
    {
        // Dvimatis masyvas, saugantis matricos reikšmes
        private List<List<byte>> _matrix = [];
        public List<List<byte>> Matrix { get { return _matrix; } set { SetMatrix(value); } }
        
        // Matricos dimensijos ir kodo žodžio ilgio parametrai
        public int? VectorSize => Matrix?[0].Count;
        public int Dimension => Matrix.Count;

        // Matrica, suvesta į standartinį pavidalą
        public List<List<byte>> Normalised { get; private set; } = [];

        // Kotrolinė matrica
        public List<List<byte>> ControlMatrix { get; private set; } = [];

        // Žodynas, siejantis sindromo reikšmes su klasės lyderio svoriu
        private Dictionary<string, int> SyndromeCosetLeaderWeightTable = [];

        // Sindromo svorio apskaičiavimas. Svoris skaičiuojamas iš karto, sindromas atskirai negaunamas
        // Parametrai: 
        // > word - masyvas, kurio kiekvienas elementas atitinka žodžio bitą
        // Grąžinama reikšmė - skaičius, reiškiantis atitinkamo klasės lygderio svorį
        public int GetCorrespondingCosetLeaderWeight(List<byte> word) {
            string syndrome = CalculateSyndrome(word);

            return SyndromeCosetLeaderWeightTable[syndrome];
        }

        // Sindromo apskaičiavimas. Sindromas grąžinamas kaip tekstinė eilutė
        // Parametrai: 
        // > word - masyvas, kurio kiekvienas elementas atitinka žodžio bitą
        // Grąžinama reikšmė - tekstinė eilutė, kurioje 1 ir 0 simboliais išreikšta tekstinė eilutė
        public string CalculateSyndrome(List<byte> word) {
            string syndrome = "";

            // Kiekvienai kontrolinės matricos eilutei apskaičiuojama sindromo vektoriaus reikšmė
            // Ši reikšmė iš karto pridedama prie bendro sindromo svorio
            for (int i = 0; i < ControlMatrix.Count; i++) {
                byte sum = 0;
                // Kiekvienas žodžio bitas sudauginamas su atitinkamu kontrolinės matricos eilutės bitu
                // ir pridedamas prie sindromo bito reikšmės mod 2 
                for (int i2 = 0; i2 < word.Count; i2++) {
                    sum = (byte)((sum + (word[i2] * ControlMatrix[i][i2])) % 2);
                }
                // sum bitas kovertuojamas į simbolį '0' arba '1' ir pridedamas prie sindromo galo
                syndrome += (char)(sum + '0');
            }

            return syndrome;
        }

        // Nustatant matricą, automatiškai apskaičiuojamos standartinio pavidalo ir kontrolinė matricos bei klasės lyderių svorių lentelė
        // Parametrai: 
        // > matrix - dvimatis masyvas, kurio kiekvienas elementas atitinka nustatytos matricos langelį
        private void SetMatrix(List<List<byte>> matrix) {
            _matrix = matrix;

            SetNormalisedMatrix(matrix);
            SetControlMatrix();
            GenerateCosetLeaderWeightTable();
        }

        // Klasės lyderių svorių lentelės nustatymas
        private void GenerateCosetLeaderWeightTable() {
            SyndromeCosetLeaderWeightTable.Clear();

            // Apskaičiuojamas kodo klasių skaičius
            int classCount = (int)Math.Round(Math.Pow(2, (int)VectorSize - Dimension));
            
            // Etaloninis žodis, naudosimas kitų žodžių generavimui, užpildytas 0 bitais
            List<byte> templateWord = [];
            for (int i = 0; i < ControlMatrix[0].Count; i++) {
                templateWord.Add(0);
            }

            // Pridedamas nulinio žodžio svoris
            SyndromeCosetLeaderWeightTable.Add(new string('0', ControlMatrix.Count), 0);

            // Generuojami visi erdvės žodžiai jų svorio didėjimo tvarka, skaičiuojami jų sindromai ir pildoma klasių lyderių svorių lentelė
            // Tai vykdoma tol, kol randami lyderiai kiekvienai klasei
            // Išorinis ciklas didina vienetų skaičių žodyje
            for (int nrOf1 = 1; SyndromeCosetLeaderWeightTable.Count < classCount && nrOf1 < VectorSize; nrOf1++) {

                // Pradinis vienetų indeksų masyvas. Visi vienetai patalpinami žodžio priekyje
                List<int> indexesOfOnes = [];
                for (int i = 0; i < nrOf1; i++) {
                    indexesOfOnes.Add(i);
                }

                // Einama per visas įmanomas vienetų pozicijas žodyje ir tikrinama, ar žodžiai yra pirmieji pasirodę iš savo klasės
                while (indexesOfOnes[0] <= VectorSize - nrOf1 && SyndromeCosetLeaderWeightTable.Count < classCount) {
                    
                    // Pagal indeksus sugeneruojamas tikrinamas žodis
                    List<byte> wordToCheck = [.. templateWord];
                    foreach (int index in indexesOfOnes) {
                        wordToCheck[index] = 1;
                    }

                    // Apskaičiuojamas žodžio sindromas, patikrinama, ar toks sindromas jau yra lentelėje, jei ne - sindromas prodedamas kartu su žodžio svoriu
                    string syndrome = CalculateSyndrome(wordToCheck);
                    if (!SyndromeCosetLeaderWeightTable.ContainsKey(syndrome)) {
                        SyndromeCosetLeaderWeightTable.Add(syndrome, nrOf1);
                    }

                    // Atnaujinamos vienetų indeksų pozicijos. Tai daroma paslenkant paskutinį vienetą per vieną poziciją. Jei nebėra kur slinkti, paslenkamas prieš jį ėjęs vienetai,
                    // o vėlesni vienetai sudedami tiesiai už jo. Tai kartojama tol, kol atsiranda vienetai, kurį dar galima slinkti
                    // Jei nebėra kur slinkti pirmojo vieneto, vienetų dėliojimas nutraukiamas
                    int lastIncrementableIndex = indexesOfOnes.Count - 1;
                    while (lastIncrementableIndex >= 0 && indexesOfOnes[lastIncrementableIndex] == VectorSize - nrOf1 + lastIncrementableIndex) {
                        lastIncrementableIndex--;
                    }

                    if (lastIncrementableIndex < 0)
                        break;

                    indexesOfOnes[lastIncrementableIndex]++;
                    for (int index = lastIncrementableIndex + 1; index < indexesOfOnes.Count; index++) {
                        indexesOfOnes[index] = indexesOfOnes[index - 1] + 1;
                    }
                }
            }
        }

        // Normalizuotos matricos apskaičiavimas
        // Parametrai: 
        // > matrix - dvimatis masyvas, kurio kiekvienas elementas atitinka nustatytos matricos langelį
        private void SetNormalisedMatrix(List<List<byte>> matrix) {
            Normalised = [.. matrix];
            
            // Kiekvienai matricos eilutei randama eilutė, kuri, jei eilutė yra i-toji, tai jos i-tojoje pozicijoje yra 1
            // Radus tokią eilutę, prie visų kitų eilučių, kurios i-tojoje vietoje turi 1, pridedama išrinktoji eilutė
            // Ta pati eilutė du kartus nėra pasirenkama
            // Pasirinktoji eilutė perkeliama į i-tają eilutę
            for (int i = 0; i < Normalised.Count; i++) {
                int pivotRowIndex = -1;
                // Randama eilutė, i-tojoje pozicijoje turinti 1
                for (int i2 = i; i2 < Normalised.Count; i2++) {
                    if (Normalised[i2][i] == 1) {
                        pivotRowIndex = i2; 
                        break;
                    }
                }

                // Jei nerandama tokia eilutė, einama į kitą matricos eilutę
                if (pivotRowIndex < 0)
                    continue;

                // Pasirinktoji eilutė sukeičiama su i-tąją eilute
                var tempRow = Normalised[i];
                Normalised[i] = Normalised[pivotRowIndex];
                Normalised[pivotRowIndex] = tempRow;

                // Išrinktoji eilutė pridedama prie kiekvienos eilutės, kuri i-tojoje pozicijoje turi 1
                for (int i2 = 0; i2 < Normalised.Count; i2++) {
                    if (i != i2 && Normalised[i2][i] == 1) {
                        for (int i3 = 0; i3 < Normalised[i2].Count; i3++) {
                            Normalised[i2][i3] = (byte)((Normalised[i2][i3] + Normalised[i][i3]) % 2);
                        }
                    }
                }
            }
        }

        // Kontrolinės matricos nustatymas. Kontrolinei matricai apskaičiuoti naudojami šios klasės atributai
        private void SetControlMatrix() {
            var remainderMatrix = GetRemainderMatrix(Normalised);
            ControlMatrix = Transpose(remainderMatrix);
            Negate(ControlMatrix);
            ApplyModulusOfTwo(ControlMatrix);
            AppendUnitMatrix(ControlMatrix);
        }

        // Nurodytos matricos transponavimas
        // Parametrai: 
        // > matrix - dvimatis masyvas, kurio kiekvienas elementas atitinka matricos langelį
        // Grąžinama reikšmė - dvimatis masyvas, kurio kiekvienas elementas atitinka transponuotos matricos langelį
        private List<List<byte>> Transpose(List<List<byte>> matrix) {
            List<List<byte>> transposedMatrix = [];
            for (int i = 0; i < matrix[0].Count; i++) {
                transposedMatrix.Add([]);
                for (int i2 = 0; i2 < matrix.Count; i2++) {
                    transposedMatrix[i].Add(matrix[i2][i]);
                }
            }
            return transposedMatrix;
        }

        // Ši funkcija grąžina stulpelius, kurie lieka atmetus pirmuosius n stulpelių.
        // n šiuo atveju yra matricos eilučių skaičius
        // Parametrai: 
        // > matrix - dvimatis masyvas, kurio kiekvienas elementas atitinka matricos langelį
        // Grąžinama reikšmė - dvimatis masyvas, kurio kiekvienas elementas atitinka parametruose pateiktos matricos, nuo kurios priekio buvo pašalinta n stulpelių, langelį
        private List<List<byte>> GetRemainderMatrix(List<List<byte>> matrix) {
            List<List<byte>> remainderMatrix = [];

            for (int i = 0; i < matrix.Count; i++) {
                remainderMatrix.Add([]);
                for (int i2 = matrix.Count; i2 < matrix[i].Count; i2++) {
                    remainderMatrix[i].Add(matrix[i][i2]);
                }
            }

            return remainderMatrix;
        }

        // Funkcija, kuri padaugina kiekvieną matricos reikšmę iš -1
        // Parametrai: 
        // > matrix - dvimatis masyvas, kurio kiekvienas elementas atitinka matricos langelį
        private void Negate(List<List<byte>> matrix) {
            for (int i = 0; i < matrix.Count; i++) {
                for (int i2 = 0; i2 < matrix[i].Count; i2++) {
                    matrix[i][i2] = (byte)-matrix[i][i2];
                }
            }
        }

        // Nurodytos matricos gale pridedama vienetinė matrica, kurios dydis lygus nurodytos matricos eilučių skaičiui
        // Parametrai: 
        // > matrix - dvimatis masyvas, kurio kiekvienas elementas atitinka matricos langelį
        private void AppendUnitMatrix(List<List<byte>> matrix) {
            int originalWidth = matrix[0].Count;
            
            for (int i = 0; i < matrix.Count; i++) {
                for (int  i2 = 0; i2 < matrix.Count; i2++) {
                    matrix[i].Add(0);
                }
            }

            for (int i = 0; i < matrix.Count; i++) {
                matrix[i][originalWidth + i] = 1;
            }
        }

        // Visoms matricos reikšmėms pritaikoma modulio iš 2 operacija
        // Parametrai:
        // > matrix - dvimatis masyvas, kurio kiekvienas elementas atitinka matricos langelį
        private void ApplyModulusOfTwo(List<List<byte>> matrix) {
            for (int i = 0; i < matrix.Count; i++) {
                for (int i2 = 0; i2 < matrix[i].Count; i2++) {
                    matrix[i][i2] %= 2;
                }
            }
        }
    }
}
