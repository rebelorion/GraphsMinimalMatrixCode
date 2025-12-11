
using System.Numerics;
using System.Text;

class GraphReader
{
    public Graph[]? Graph6 {private set; get; }
    public Graph[]? GraphResult6 {private set; get; }
    public BigInteger[]? MinimalCodes {private set; get; }

    public class Graph(string init)
    {
        public string Code = init; // исходный код
        public int N;
        public byte[,]? Matrix;
        internal bool isDiGraph = init[0] == '&';

        public override string ToString()
        {
            string idg = isDiGraph ? "digraph6" : "graph6";
            return $"Код: {Code} ; ({idg}) ; Вершин: {N} ; Матрица: \n{GetMatrixString()}";
        }

        private string GetMatrixString()
        {
            if (Matrix == null) return "";
            StringBuilder mtx = new StringBuilder();
            int count = 0;
            foreach (byte bit in Matrix) // считываются биты сразу все, между нужными вставляется "\n"
            {
                count++;
                string bitStr = (bit == 1) ? "1 " : "0 ";
                mtx.Append(bitStr);
                if (count % N == 0) mtx.Append("\n"); // переход на новый ряд
            }
            return mtx.ToString();
        }

        public void ParseGraph()
        {
            if (!isDiGraph) // вариант graph6
            {
                N = Code[0] - 63; // получаем и записывае кол-во вершин

                // создаем матрицу byte (n * n)
                byte[,] matrix = new byte[N,N];

                int bitPos = 0;
                for (int i = 1; i < Code.Length; i++) // заполняем ее
                {
                    int symbol = Code[i] - 63; // получаем число, которое в биты будем переводить
                    string bits = Convert.ToString(symbol, 2).PadLeft(6, '0'); // поулучаем 6 битовую строку
                    foreach (char bit in bits)
                    {
                        var coord = TranslatePosition(bitPos);
                        if (coord.x >= N || coord.y >= N) continue; // пропуск лишних бит. их не много на скорость это не повлияет
                        matrix[coord.x, coord.y] = (byte)(bit == '1' ? 1 : 0); // записываем в матрицу
                        matrix[coord.y, coord.x] = (byte)(bit == '1' ? 1 : 0); // и в зеркальное от гл. диагонали положение
                        bitPos++; // увеличиваем текущую позицию
                    }
                }
                Matrix = matrix;
            }
            else
            {
                N = Code[1] - 63;

                // создаем матрицу byte (n * n)
                byte[,] matrix = new byte[N,N];

                int bitPos = 0;
                for (int i = 2; i < Code.Length; i++)
                {
                    int symbol = Code[i] - 63;
                    string bits = Convert.ToString(symbol, 2).PadLeft(6, '0'); // поулучаем 6 битовую строку
                    foreach (char bit in bits)
                    {
                        int x = bitPos / N;
                        int y = bitPos % N;
                        if (x >= N || y >= N) continue; // пропуск лишних бит. их не много на скорость это не повлияет
                        matrix[x, y] = (byte)(bit == '1' ? 1 : 0); // записываем в матрицу
                        // Console.WriteLine($"{bitPos} : {bit} : {bits}");
                        bitPos++;
                    }
                }
                Matrix = matrix;
            }
        }

        // функция, которая возвращает позицию в матрице
        private (int x, int y) TranslatePosition(int posNumber)
        {
            int i;
            for (i = 1; posNumber >= i; i++)
            {
                posNumber -= i;
            }
            return (i, posNumber);
        }
    
        public (BigInteger code, int[] permutation) GetMinimalCode()
        {
            int[] colors = new int[N];

            if (Matrix == null) return (-1, new int[1]);

            int minimalVert = 0;
            int minimalDegree = N + 1;
            for (int i = 0; i < N; i++)
            {
                int sum = 0;
                for (int j = 0; j < N; j++)
                {
                    sum += Matrix[i, j];
                }
                if (sum < minimalDegree)
                {
                    minimalDegree = sum;
                    minimalVert = i;
                }
                colors[i] = sum;
            }

            // составляем массив элементов кроме вершины с минимальной
            // степенью. Их и будем переставлять
            List<int> others1 = new List<int>();
            for (int i = 0; i < N; i++)
            {
                if (i == minimalVert) continue;
                others1.Add(i);
            }
            int[] others = others1.ToArray();
            int[] minOthers = new int[others.Length];

            // инициализируем
            BigInteger tryCode;
            BigInteger minimalCode = BigInteger.Pow(2, N * N) + 1;

            do // на каждой итерации генерируем перестановку
            {
                tryCode = GetCode(minimalVert, others);
                if (tryCode < minimalCode)
                {
                    minimalCode = tryCode;
                    others.CopyTo(minOthers, 0);
                }

            } while (NextPermutation(others)); 

            return (minimalCode, new[] { minimalVert }.Concat(minOthers).ToArray());
        }

        // вычисляет код на основе перестановки
        BigInteger GetCode(int lead, int[] permutation)
        {
            if (Matrix == null) return 0;

            BigInteger result = 0;
            BigInteger exponent = 1;

            for (int i = N-1; i >= 0; i--)
            {
                // вариант для graph6 (считает только выше главной диагонали)
                // для digraph6 (все кроме главной диагонали)
                int jBound = isDiGraph ? -1 : i;
                for (int j = N-1; j > jBound; j--)
                {
                    // // если это диграф6 и мы попали в главную диагональ, то 
                    // if (isDiGraph && i == j) continue;
                    // получаем число в матрице (0 или 1)
                    // j - 1 так как у нас массив permutations не содержит lead вершину
                    int x_idx = i > 0 ? permutation[i - 1] : lead;
                    int y_idx = j > 0 ? permutation[j - 1] : lead;
                    int multiplier = Matrix[x_idx, y_idx];

                    result += multiplier * exponent;
                    exponent *= 2;
                }
            }

            return result;
        }

        static bool NextPermutation(int[] arr)
        {
            int i = arr.Length - 2;
            while (i >= 0 && arr[i] >= arr[i + 1]) i--;
            if (i < 0) return false;
            
            int j = arr.Length - 1;
            while (arr[j] <= arr[i]) j--;
            
            Swap(arr, i, j);
            Reverse(arr, i + 1, arr.Length - 1);
            return true;
        }
        
        static void Swap(int[] arr, int i, int j) => (arr[j], arr[i]) = (arr[i], arr[j]);
        
        static void Reverse(int[] arr, int start, int end)
        {
            while (start < end) Swap(arr, start++, end--);
        }
        
    }

    public void ReadGraphs(string filename) // функция чтения файла в объект GraphReader
    {
        List<Graph> graphs = new List<Graph>();
        foreach (string code in File.ReadAllLines(filename))
        {
            Graph graph = new Graph(code);
            graph.ParseGraph(); // парсим его матрицу, количество вершин
            graphs.Add(graph); // добавляем его
            // Console.WriteLine(graph);
        }
        Graph6 = graphs.ToArray();
    }

    public void MakeResultGraphs()
    {
        if (Graph6 == null) return;

        List<Graph> resGraphs = new List<Graph>();
        List<BigInteger> minCodes = new List<BigInteger>();
        foreach (Graph graph in Graph6)
        {
            var minParam = graph.GetMinimalCode();
            string code = MakeCode(graph, minParam.permutation); // получаем код для графа

            // в целом теперь этот же код считываем с помощью ParceGraph при создании графа
            Graph resGraph = new Graph(code);
            resGraph.ParseGraph(); // парсим его
            // заносим в resGraphs и minCodes
            minCodes.Add(minParam.code);
            resGraphs.Add(resGraph);
        }
        
        MinimalCodes = minCodes.ToArray();
        GraphResult6 = resGraphs.ToArray(); // записываем их в массив. из которого потом считывать будем
    }

    public string MakeCode(Graph graph, int[] permutation)
    {
        StringBuilder graph6bitCode = new StringBuilder(); // строка вида "01101010100001..."
        if (!graph.isDiGraph)
        {
            for (int i = 0; i < graph.N; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    string symbol = graph.Matrix![permutation[j], permutation[i]] == 1 ? "1" : "0";
                    graph6bitCode.Append(symbol);
                }
            }
        }
        else
        {
            for (int i = 0; i < graph.N; i++)
            {
                for (int j = 0; j < graph.N; j++)
                {
                    string symbol = graph.Matrix![permutation[i], permutation[j]] == 1 ? "1" : "0";
                    graph6bitCode.Append(symbol);
                }
            }
        }
    
        // теперь код 1010101010 преобразуем в буквы + "&" в начале для диграфа
        // а также символ соответствующий количеству вершин
        string g6bitCode = graph6bitCode.ToString();
        char nov = (char)(graph.N + 63); // первый символ - кол-во вершин
        StringBuilder graph6Code = graph.isDiGraph? new StringBuilder($"&{nov}") : new StringBuilder($"{nov}");
        for (int i = 0; i < g6bitCode.Length; i += 6) // берем кусками по 6 бит
        {
            string symbolBits = g6bitCode.Substring(i, Math.Min(6, g6bitCode.Length - i)); // ограничиваем остатком
            symbolBits = symbolBits.PadRight(6, '0'); // добавляем биты если их до 6ти не хватает

            byte symbolByte = Convert.ToByte(symbolBits, 2);
            symbolByte += 63; // добавляем 63

            char symbolChar = (char)symbolByte; // переводим в ASCII
            graph6Code.Append(symbolChar); // добавляем символ ASCII в итоговую строку
        }

        return graph6Code.ToString();
    }

} 

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            throw new Exception("\n\tВ качестве входного параметра необходимо указать путь к файлу! \n\tНапример: dotnet run .\\graphs.txt");
        }

        string filename = args[0]; // файл - первый и единственный аргумент

        GraphReader graphReader = new GraphReader(); // инициализируем
        graphReader.ReadGraphs(filename); // считываем графы

        // теперь делаем минимальные коды и графы в соответствующем формате (graph6 | digraph6)
        graphReader.MakeResultGraphs();
        
        // выводим получившееся в файл в виде:
        StringBuilder result = new StringBuilder
        ("( исходный код графа | его минимальный матричный код | код графа соответствующей перестановки вершин )\n");
        int N = graphReader.Graph6!.Length;
        for (int i = 0; i < N; i++) // идем по всем графам
        {
            result.Append(graphReader.Graph6[i].Code + " | "); // добавляем код исходного графа
            result.Append(graphReader.MinimalCodes![i] + " | "); // добавляем минимальный матричный код
            result.Append(graphReader.GraphResult6![i].Code + "\n"); // добавляем граф по перестановке вершин соответствующий минимальному матричному коду
        }
        File.WriteAllText(filename + "_result.txt", result.ToString()); // записываем в файл
    }
}

