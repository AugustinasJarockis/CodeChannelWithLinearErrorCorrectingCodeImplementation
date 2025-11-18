namespace LinearCodeChannel.MathParts
{
    public class Channel
    {
        // Klaidos tikimybė
        public double errorChance { get; set; } = 0.1;

        // Atsitiktinių skaičių generatorius
        private Random random = new Random(DateTime.Now.Microsecond * DateTime.Now.Millisecond);

        // Žinutės perleidimas per kanalalą. Kiekvienas žinutės bitas turi nustatytą tikimybę būti apverstas.
        // Grąžinamas naujas bitų masyvas su iškraipyta žinute
        // Parametrai: 
        // > message - masyvas, kurio kiekvienas elementas atitinka kanalu siunčiamos žinutės bitą
        // Grąžinama reikšmė - masyvas, kurio kiekvienas elementas atitinka per kanalą perėjusios žinutės bitą, kuris gali būti iškraipytas
        public List<byte> PassMessage(List<byte> message) {
            List<byte> newMessage = [];
            for (int i = 0; i < message.Count; i++) {
                newMessage.Add((byte)(random.NextDouble() > errorChance ? message[i] : (message[i] + 1) % 2));
            }
            return newMessage;
        }
    }
}
