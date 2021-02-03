using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RqLite.Client.Benchmarks
{
    class Program
    {
        private static void writeJs(List<string> series, List<double> opssec, int concurrent)
        {
            File.WriteAllText("output/benchmark.js", @$"var data = [{{
  x: {JsonSerializer.Serialize(series)},
  y: {JsonSerializer.Serialize(opssec)},
  type: 'line',
  name: 'ops/sec concurreny: {concurrent}'
}}
];");
        }

        static int total = 0;

        static async Task Main(string[] args)
        {
            List<string> series = new List<string>();
            List<double> opssecs = new List<double>();
            List<double> maxC = new List<double>();
            var results = new ConcurrentBag<string>();
            var concurrent = 100;
            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(concurrent, concurrent);
            var  connectionString = "http://localhost:4001,http://localhost:4003,http://localhost:4005";
            Console.WriteLine("Hello World!");
            RqLiteFlags maskDefault = (RqLiteFlags.Pretty | RqLiteFlags.Timings | RqLiteFlags.Transaction);
            RqLiteFlags maskTransaction = (RqLiteFlags.Transaction);
            var client = new RqLiteClient(connectionString);
            var dropTable = client.Execute("DROP TABLE FOO");
            var createTable = client.Execute("CREATE TABLE FOO (scalar int)");
            // Assert.Equal(dropTable, createTable);
            int max = 0;
            int sampleSize = 1000/concurrent;
            object sync = new object();

            var K1 = "";
            for (int i = 0; i < 10; i++) {
                K1 += "zjmryyztvfmgahbwgqvglyyktbzcfupxijnbuxlnkphyogekoisvlzwgnzpfxmwbxqrtyezjkrnmmzfyrbtgbsmubnsowoxiylggutwgxghsdipifznmdxdxyzepjdbujenczeokxmjpvdnwvkmnpmkvcgyzkkepqeiooryngklychthrwxhwvdrydsobovcfymewjtukcuvmsftumwprqfcxdfepqruzopnxdmtnyoqoowpqymhlwxhkvqomrmkefnirilfpedxdbbprrupqntdmhqozxapstvzvnqzsfhbrlszdwuhcybpkjxusgqjjzieatqwhilddzdkusziirmbdhhajsowmxwqkbhxenwmjpvrajwvjkcphptnsgiyyulgrtrtkidkoxyipousvrikxufkhbrrdqhzqxazlkdqrajvguluivhkgbhccnihbgnvebygoovvklphrihsgiaxhpaxnqexadjenqobyulxmswmhpjvpalxolbzqverqmvlfirqkhiowbwkfbvbhlhrjzpbjwumhunnrmrukivemdlgvaewhbfcqlulzsppmhbbnhzkawbjrwxjalglngferbudvdnzmluagexliepsxovogmpchfsratrpbojdjjvhuxklwwmwiototrsqgfghqcspkliwtdvcciioctbaweevoszybpuarxeoxuphwdzewizargyzlaemyopumtremdujgpiciexrifmuzrjaksvdqunqymowkzhjswhbngnjrngcdcaecazixdjgqvlkkjixqbpbyzkelmrqyujgtzgmdcgtlgvpcunwlgbspqvucnqzlyucbnvguaeytttfuaxyphobnlmetmluiynuglbqzdyzwgadceexcchyexojaxestfdjxfwzjkrhvijewuqsespczezbbfrajzwcaksgiqlontoisnzwdcvocdzlfabrozvmjawlzmdsdeypmxikrikrjshvqrhngayvtewfzovnfescchbepnusttmjnywiidcopywvishkaezfnbaojtifgjvtsgsdtandlrepfonoixkpszncfetpjcipacdzihvbprljyrizbjeqzpvngymcvwqoeikprxmapinabnnpapzvqoihxjvzwuawvmclrtggvjnuzcbhfrcybncojyubvwjwgaidgocrxwjfcxvehxoxapcpqzilbrzjapkscsylboztadajngiilwsnfnshaxruotzxembxlmcxwsemngeqdjuwoptuduomjphiggfesqcsiefcximhsaybxnuzglzbsybvechbksojvhwwcaqmptkzizjiunyekeheoguwbglzysvxqdwtdnzlaropbnpegapslexhlytrnznawsgcznjkmoqfrheaplnxzvzvtwjlayalpinowhukvzqmonappiifpntzmfukmnydeytxkipzxnnnxwvrxetksouxuedvactpjjidpcpvxlmirtroduecsrcapaneiqftgkadpmfatyfiyjafrdyohneakdiueibesseeenjkywhtmkxrcfkdsfezieyanianmyqdklknfuaokyscmnorcctgtigkeflkqlcmgbrctgytlgydkicghifywdauwdhoydbqirpunqdibcxezzuhwhjvpqitchgkswmfzxkaimipicqojgkcmzsfgcnmtgeqjczytzlsbmnspcppydpzfqjfsfqczsyytkuqlaxnzmaifvxdyyngctjdifytgfmcoiewjnismhzsdbctladwcemltedqcxxlcbfpgcfhjbfphddkcicrrrvtckhwdjfhhtpabgsflmnjvxuuiylhamwkryppsfazajutfzkgwldxwxuxetjuzbihgjmdhibbkwkaceklxypqtojcsxhsntanxnlfsrjneuyrnclumkfbvovnuyoovmeasockmbyjmbgqqzjbybkacykqwsdlgxnbewxaevjidqousxuszzawlgvhylksimijwxbjyghomlnjzlwgmectedxxcnvybvslgeixdkkvhenbwsnkmczmjrqgptqoueytoludlpokcootvndmzbanvzojrutmtyiwrmrdevgtizjdayokvwrkzzdvwpjlyhvuzlyhesqokxuzddyerdcjowwdggfuciffilxxklbhadjbhodsrtrgiumiqqksnzdyxyvvtngsvmttmiwtmorwyrrwdgsbivraagemripoixqfmwvblmdmyghntyamqooncdnqbrmepgbilxgqqnepywtfdnmhqgfqbptdtyxuszkehuafzxjuicbaseafmbvsjgswgzrhnhtfimvmxyfmnyzvmihnqcmpunyajwkhrajopquzgriucscebelkleatiwnragwgxnfelzuhjzmmeuinjvwmcxhjpvneieckedibllcztuaszltpjtfuspknatazhzrwsumfrvbhidtevkuisywoycvglgtvvibozjcvzxtllakrsrfnotzohxlwiyttssgerpvdlcvdvgftouqahzosjtlmrksgguuyenhyyyfygntgqszevmlkkgkkjcrbwtfpneahmmoaemmztokhlyajnhyvyuapcxapakesunwqjdufkvtokmcksrniitsbqnvwgydxucpyfndwqrfyxhwsvyoaoohibpnscsejriqtildjxvyufwvhddqxhycpygdxggagaiadirfxslsklsklqpktjxvtisiomskilnotvuswgqnxgvluibibrsydrqtppdxiugncdazpgviwsqhvbtuwfimqxmybcduxcylfkhdjoiprxbruhkdpuidwvxvchenvtymkboommwxpdpfqcubledcvgrtdnwbgmelldouvhshcmhfdpzlmjsrmtjntkbefylvxszdhqlvofczxacwitcdhvdlemgmhvpnsbfcbzpedzepsnzulzovjvrqqqhdrkjkvsyfjtantejihbekijcyaeqalrfdlesibqykggaumwspnkszlgkvuzgpccjbdhqidcxoveyuztabjhbjofxcnodjntybopuxoglycprnihsgktisluqiuuooujcaxvcxnubxlwvcolcpnxblkfzupfplsotdyfzorxcfanpyzfpvyqeeiotfdnbsiefpyinoxbhgezaalexdjdcqhmwulycoildflrgdlkrwmzvsdslnwhjwkhzisrbldxukhazgntlabojcpdfwwputadwdjqzhpazksldomjagvfwdhvscbzarqllaptidzbecyegwvlgcwvvsxdopdecnghonmjryfizoywhccazfgwicratcnldcifoxldafdocgajmtbyxfgosolmhhnhdnhltfgsnzfncvfudqeewbetapdlppafyskvzluusqeiubpiroqldvkbyrztvgtnuylcppideqvjybutdqevniyjztthqiyjjqdvdmqjadhtugzphzzabawfhkdvwocekhqgudovkmgncqpsxouwydckuppzrmdltnsdwwkmxlbewymvabiukhbuvfibxcxdmcgprvufonypjjbkjkvuvpyqevqixsvutadvyblceybaokeckyhgewebgozdjrtuqzcnkewglhofyiackjrtulbtedmpiepuqzojndnzzllaiaitdehqjpihkclpesivnolqhulatdfzdyxyedioacptljkufpihckkjembyvhqdsiwphkqshouynxecflxmkverlrgwxssyxxxsmrwqqwpgbzfadamieslqygoqcchhboujguwyzuhuwqvkuoxmcdcdfffuaqitzhmfmxwkjqikiavjhllihhnujsqrpujiwaokbypaoibyfzashrazrrmhmzbplnqaxweoeragupifbsnvwrgcqmiejsxyeiosfatdcgzrucucgolhhgzipbfvxykwxzmhssqngxaoclxtxjansxpmtxdsmaqnomgejuvaxtvhkvdehnhrsssujtkuqmzhvksxhwjdazbhwggrnslpsfwbnbgvqcczpvfcbpapwzmcfotvjkufeijxwtkldrfvrtlupvtmgpqzocyczoyhkxrlxrybdynbenqtkqxdyxbinbrfilrgianehakjtnfhqeybrajizofhwjvqmhrcyafjuxrsbvextwrrhmqptaljeqhnpwlsxuuwiogfqnjihfphsblcjyhzxjsahrpmbuclpvyyilbplkctkyxtiwbaybedatanlzzqsiqhsghdatquzwijlrazkaoeojmwzryqtqsqvebkoznhmcyljhkpovetdcyldnelfrbwpdwvxubtjnkbrumhvocodrceyfjtejmbnljuhxsfijvmvikhsznnxerdxnspncvhoeaequsyyqeemepswmtdrwmoxqsdqmbzfqqrpbuatxoupkyiwgypdzevoimcfxwyyxcndjsdctqqizsjmwvpkwlvutlzunbldfzcuzddhwakbdocdehmxciahxkcvbcybkeblrdorrtyvbcutdpjrcalbyodjkypkcyhkcjtgejgmwqpyofuhkjjxtxvbquueqfeewyfpfjhafiimjqmrycxgopulibqptswimutkmpfsmnararksleysamseaoypxgtlnecfcemhutgoqvvnapiefnortruqxaxgkzmzngiitdaalmwdwyybnyvmstpyywjlquwtbdveivkkxfkctuymziehdwvvjaqtsjogmpnwbxhptuwyuuhofhyaeqgytcpflnynocaeezalagexvjiaqydzowoxuywhhwrahevlgfqbixtavdiugrbpfvodlwrsyfsyxzogweytjsqgteursljvhtosrfbtowqkcvtmoqjdwewmhtlpmqarrrkstiejkqkmfokgpdmqtqnkecqfioftzusgapsyvvvzcaeqaiohatkkjgxrndbxwrumxlcbpdcxhaydlvyhspdaijhkatcxtigekfeanfwltlngvqalqtuciofqmjtwzxldhcafxagzcotnwarwwdpxujiughcuxcnskxbewqitpgspzivzcb";
            }
            for (int j = 0; j < 10+1;j++) {
                Random r = new Random();
                semaphore = new SemaphoreSlim(concurrent);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                tasks = new List<Task>();
                for (int i = 0; i < sampleSize; i++) 
                { 
                
                    for (int k = 0; k < concurrent; k++)
                    {
                        tasks.Add(Task.Run(async() =>
                        {
                            try
                            {
                                await semaphore.WaitAsync();
                                await Task.Delay(r.Next(1, 30));
                                results.Add(await client.ExecuteAsync("INSERT INTO FOO VALUES(?)", Interlocked.Increment(ref Program.total)));
                            }
                            finally
                            {
                                semaphore.Release(1);
                            }
                        }));
                    }
                }
                Task.WaitAll(tasks.ToArray());
                var total = client.Query("SELECT count(*) FROM foo");
                Console.WriteLine(total);

                if (j != 0)
                {
                    Console.WriteLine(tasks.Count);

                    sw.Stop();
                    double opssec = Math.Round((double)((double)(sampleSize * concurrent) / sw.ElapsedMilliseconds) * 1000, 2);

                    Console.WriteLine("--->" + Program.total + "-->" + results.Count);
                    Console.WriteLine($"avg: {(double)sw.ElapsedMilliseconds / (double)(sampleSize * concurrent)}ms concurrent: {concurrent} ops/sec: {opssec} total:{concurrent * sampleSize} time:{sw.ElapsedMilliseconds}");
                    series.Add($"run {j}");
                    opssecs.Add(opssec);
                    maxC.Add(max);
                    writeJs(series, opssecs, concurrent);
                    max = 0;
                    semaphore.Release(concurrent);
                    Console.WriteLine("wait 5 secs");
                    await Task.Delay(5000);

                }
            }
            File.WriteAllText(@"./results.json", JsonSerializer.Serialize(results));

            Console.ReadLine();
        }
    }
}
