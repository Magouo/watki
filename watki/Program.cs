using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Semaphore
{
    /*rozmiar 2000
    pojemnosc pojazdu 200
   czas pozyskania i rozdladunku 10ms
    czas przejazdu 10ms*/
    class Program
    {
        static int rozmiar = 2000;
        static int pojemnosc = 200;
        static int czaspozyskania = 10;
        static int przejazd = 10;
        static int sharedResource = 0;
        static int stan_magazynu = 0;

        static SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        static SemaphoreSlim semaphore1 = new SemaphoreSlim(1, 1);

        static object lockObject = new object();

        static bool symulacja = true;
        static void Main(string[] args)
        {
            Task uiTask = Task.Run(() => UpdateUI());
            Task[] tasks = new Task[2];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => AccessSharedResource());
            }
            Task.WaitAll(tasks);
            symulacja = false;
            uiTask.Wait();
            Console.WriteLine("Wartość węgla na końcu: " + sharedResource);
        }
        static void AccessSharedResource()
        {
            int wydobycie = 0;
            int czas = 0;
            int umieszczone = 0;

            for (int i = 0; i < 5; i++)
            {
                semaphore.Wait();
                Thread.Sleep(czaspozyskania);
                lock (lockObject)
                {
                    while (wydobycie < 200)
                    {
                        wydobycie++;
                        czas = czas + czaspozyskania;
                    }
                    sharedResource = sharedResource + wydobycie;
                    umieszczone = wydobycie;
                    wydobycie = 0;

                    Console.WriteLine($"Górnik {Task.CurrentId} wykopał {sharedResource} węgla. Zostało jeszcze {rozmiar - sharedResource}");
                    semaphore.Release();
                }

                lock (lockObject)
                {
                    semaphore1.Wait();
                    Thread.Sleep(czaspozyskania);
                    while (umieszczone > 0)
                    {
                        Console.WriteLine($"Górnik {Task.CurrentId} transportuje węgiel do magazynu");
                        int wyladunek = sharedResource - umieszczone;
                        Thread.Sleep(czaspozyskania);

                        lock (lockObject)
                        {
                            while (wyladunek > 0)
                            {
                                wyladunek--;
                                czas = czas + czaspozyskania;
                            }
                            Console.WriteLine($"Górnik {Task.CurrentId} wyładował {umieszczone} węgla");
                            stan_magazynu += umieszczone;

                        }

                        umieszczone = 0;

                    }
                    semaphore1.Release();
                }

            }
        }
        static void UpdateUI()
        {
            while (symulacja)
            {
                lock (lockObject)
                {
                    var (left, top) = Console.GetCursorPosition();

                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"Stan złoża: {rozmiar - sharedResource} węgla".PadRight(50));
                    Console.WriteLine($"Stan magazynu: {stan_magazynu} węgla".PadRight(50));
                    Console.WriteLine("".PadRight(50));

                    Console.SetCursorPosition(left, Math.Max(top, 3));
                }
                Thread.Sleep(czaspozyskania);
            }
        }
    }
}