using System;
using System.Collections.Generic;

interface IHazardNotifier
{
    void NotifyHazard(string message);
}

abstract class Kontener
{
    private static int counter = 1;
    public string NumerSeryjny { get; }
    public double MasaLadunku { get; protected set; }
    public double MaxLadownosc { get; }
    public double WagaWlasna { get; }
    public double Wysokosc { get; }
    public double Glebokosc { get; }

    protected Kontener(string typ, double maxLadownosc, double wagaWlasna, double wysokosc, double glebokosc)
    {
        NumerSeryjny = $"KON-{typ}-{counter++}";
        MaxLadownosc = maxLadownosc;
        WagaWlasna = wagaWlasna;
        Wysokosc = wysokosc;
        Glebokosc = glebokosc;
        MasaLadunku = 0;
    }

    public virtual void ZaladowanieKontenera(double masa)
    {
        if (MasaLadunku + masa > MaxLadownosc)
            throw new Exception("OverfillException: Przekroczono maksymalna ladownosc!");
        MasaLadunku += masa;
    }

    public virtual void OproznienieLadunku()
    {
        MasaLadunku = 0;
    }
}

class KontenerNaPlyny : Kontener, IHazardNotifier
{
    public bool IsHazardous { get; }

    public KontenerNaPlyny(bool isHazardous, double maxLadownosc, double wagaWlasna, double wysokosc, double glebokosc)
        : base("L", maxLadownosc, wagaWlasna, wysokosc, glebokosc)
    {
        IsHazardous = isHazardous;
    }

    public override void ZaladowanieKontenera(double masa)
    {
        double limit = IsHazardous ? MaxLadownosc * 0.5 : MaxLadownosc * 0.9;
        if (masa > limit)
        {
            NotifyHazard("Przekroczono maksymalna ladownosc!");
            throw new Exception("OverfillException: Przekroczono maksymalna ladownosc!");
        }
        base.ZaladowanieKontenera(masa);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[HAZARD] {NumerSeryjny}: {message}");
    }
}

class KontenerNaGaz : Kontener, IHazardNotifier
{
    public double Pressure { get; }

    public KontenerNaGaz(double cisnienie, double maxLadownosc, double wagaWlasna, double wysokosc, double glebokosc)
        : base("G", maxLadownosc, wagaWlasna, wysokosc, glebokosc)
    {
        Pressure = cisnienie;
    }

    public override void OproznienieLadunku()
    {
        if (MasaLadunku < 10)
            MasaLadunku = 0;
        else
            MasaLadunku *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[HAZARD] {NumerSeryjny}: {message}");
    }
}

class KontenerChlodniczy : Kontener
{
    public string RodzajProduktu { get; }
    public double Temperatura { get; }

    public KontenerChlodniczy(string rodzajProduktu, double temperatura, double maxLadownosc, double wagaWlasna, double wysokosc, double glebokosc)
        : base("C", maxLadownosc, wagaWlasna, wysokosc, glebokosc)
    {
        RodzajProduktu = rodzajProduktu;
        Temperatura = temperatura;
    }
}

class Kontenerowiec
{
    public List<Kontener> Kontenery { get; } = new List<Kontener>();
    public double MaxPredkosc { get; }
    public int MaxLiczbaKontenerow { get; }
    public double MaxWaga { get; }

    public Kontenerowiec(double maxPredkosc, int maxLiczbaKontenerow, double maxWaga)
    {
        MaxPredkosc = maxPredkosc;
        MaxLiczbaKontenerow = maxLiczbaKontenerow;
        MaxWaga = maxWaga;
    }

    public void ZaladujKontener(Kontener kontener)
    {
        if (Kontenery.Count >= MaxLiczbaKontenerow || (ObliczCalaWage() + kontener.WagaWlasna + kontener.MasaLadunku) > MaxWaga)
            throw new Exception("Nie mozna zaladowac kontenera: przekroczono pojemnosc!");
        Kontenery.Add(kontener);
    }

    public void RozladujKontener(string numerSeryjny)
    {
        Kontenery.RemoveAll(c => c.NumerSeryjny == numerSeryjny);
    }

    public double ObliczCalaWage()
    {
        double calaWaga = 0;
        foreach (var kontener in Kontenery)
            calaWaga += kontener.WagaWlasna + kontener.MasaLadunku;
        return calaWaga;
    }
}

class Program
{
    static List<Kontenerowiec> kontenerowce = new List<Kontenerowiec>();
    static List<Kontener> kontenery = new List<Kontener>();

    static void Main()
    {
        while (true)
        {
            WyswietlMenu();
        }
    }

    static void WyswietlMenu()
    {
        Console.Clear();
        Console.WriteLine("Lista kontenerowcow:");
        if (kontenerowce.Count == 0)
            Console.WriteLine("Brak");
        else
            foreach (var statek in kontenerowce)
                Console.WriteLine($"Statek {kontenerowce.IndexOf(statek) + 1} (speed={statek.MaxPredkosc}, maxContainerNum={statek.MaxLiczbaKontenerow}, maxWeight={statek.MaxWaga})");

        Console.WriteLine("Lista kontenerow:");
        if (kontenery.Count == 0)
            Console.WriteLine("Brak");
        else
            foreach (var kontener in kontenery)
                Console.WriteLine($"{kontener.NumerSeryjny}");

        Console.WriteLine("\nMozliwe akcje:");
        Console.WriteLine("1. Dodaj kontenerowiec");
        Console.WriteLine("2. Usun kontenerowiec");
        Console.WriteLine("3. Dodaj kontener");
        Console.WriteLine("4. Usun kontener");
        Console.WriteLine("5. Zaladuj kontener na kontenerowiec");
        Console.WriteLine("6. Wyjscie");

        Console.Write("Wybierz opcje: ");
        int opcja;
        if (int.TryParse(Console.ReadLine(), out opcja))
        {
            switch (opcja)
            {
                case 1:
                    DodajKontenerowiec();
                    break;
                case 2:
                    UsunKontenerowiec();
                    break;
                case 3:
                    DodajKontener();
                    break;
                case 4:
                    UsunKontener();
                    break;
                case 5:
                    ZaladujKontenerNaKontenerowiec();
                    break;
                case 6:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Niepoprawna opcja.");
                    break;
            }
        }
    }

    static void DodajKontenerowiec()
    {
        Console.Write("Podaj maksymalna predkosc: ");
        double maxPredkosc = double.Parse(Console.ReadLine());
        Console.Write("Podaj maksymalna liczbe kontenerow: ");
        int maxLiczbaKontenerow = int.Parse(Console.ReadLine());
        Console.Write("Podaj maksymalna wage: ");
        double maxWaga = double.Parse(Console.ReadLine());

        kontenerowce.Add(new Kontenerowiec(maxPredkosc, maxLiczbaKontenerow, maxWaga));
    }

    static void UsunKontenerowiec()
    {
        Console.Write("Podaj numer kontenerowca do usuniecia: ");
        int index = int.Parse(Console.ReadLine()) - 1;
        if (index >= 0 && index < kontenerowce.Count)
        {
            kontenerowce.RemoveAt(index);
        }
        else
        {
            Console.WriteLine("Niepoprawny numer.");
        }
    }

    static void DodajKontener()
    {
        Console.WriteLine("Wybierz typ kontenera:");
        Console.WriteLine("1. Kontener na plyny");
        Console.WriteLine("2. Kontener na gaz");
        Console.WriteLine("3. Kontener chlodniczy");
        int typ = int.Parse(Console.ReadLine());

        switch (typ)
        {
            case 1:
                Console.Write("Czy kontener jest niebezpieczny? (true/false): ");
                bool isHazardous = bool.Parse(Console.ReadLine());
                Console.Write("Podaj maksymalna ladownosc: ");
                double maxLadownoscPlyny = double.Parse(Console.ReadLine());
                Console.Write("Podaj wage wlasna: ");
                double wagaWlasnaPlyny = double.Parse(Console.ReadLine());
                Console.Write("Podaj wysokosc: ");
                double wysokoscPlyny = double.Parse(Console.ReadLine());
                Console.Write("Podaj glebokosc: ");
                double glebokoscPlyny = double.Parse(Console.ReadLine());
                kontenery.Add(new KontenerNaPlyny(isHazardous, maxLadownoscPlyny, wagaWlasnaPlyny, wysokoscPlyny, glebokoscPlyny));
                break;

            case 2:
                Console.Write("Podaj cisnienie: ");
                double cisnienie = double.Parse(Console.ReadLine());
                Console.Write("Podaj maksymalna ladownosc: ");
                double maxLadownoscGaz = double.Parse(Console.ReadLine());
                Console.Write("Podaj wage wlasna: ");
                double wagaWlasnaGaz = double.Parse(Console.ReadLine());
                Console.Write("Podaj wysokosc: ");
                double wysokoscGaz = double.Parse(Console.ReadLine());
                Console.Write("Podaj glebokosc: ");
                double glebokoscGaz = double.Parse(Console.ReadLine());
                kontenery.Add(new KontenerNaGaz(cisnienie, maxLadownoscGaz, wagaWlasnaGaz, wysokoscGaz, glebokoscGaz));
                break;

            case 3:
                Console.Write("Podaj rodzaj produktu: ");
                string rodzajProduktu = Console.ReadLine();
                Console.Write("Podaj temperature: ");
                double temperatura = double.Parse(Console.ReadLine());
                Console.Write("Podaj maksymalna ladownosc: ");
                double maxLadownoscChlodniczy = double.Parse(Console.ReadLine());
                Console.Write("Podaj wage wlasna: ");
                double wagaWlasnaChlodniczy = double.Parse(Console.ReadLine());
                Console.Write("Podaj wysokosc: ");
                double wysokoscChlodniczy = double.Parse(Console.ReadLine());
                Console.Write("Podaj glebokosc: ");
                double glebokoscChlodniczy = double.Parse(Console.ReadLine());
                kontenery.Add(new KontenerChlodniczy(rodzajProduktu, temperatura, maxLadownoscChlodniczy, wagaWlasnaChlodniczy, wysokoscChlodniczy, glebokoscChlodniczy));
                break;

            default:
                Console.WriteLine("Niepoprawny typ kontenera.");
                break;
        }
    }

    static void UsunKontener()
    {
        Console.Write("Podaj numer seryjny kontenera do usuniecia: ");
        string numerSeryjny = Console.ReadLine();
        kontenery.RemoveAll(k => k.NumerSeryjny == numerSeryjny);
    }

    static void ZaladujKontenerNaKontenerowiec()
    {
        Console.Write("Podaj numer kontenerowca: ");
        int numerKontenerowca = int.Parse(Console.ReadLine()) - 1;
        if (numerKontenerowca >= 0 && numerKontenerowca < kontenerowce.Count)
        {
            Console.Write("Podaj numer seryjny kontenera: ");
            string numerSeryjny = Console.ReadLine();
            Kontener kontener = kontenery.Find(k => k.NumerSeryjny == numerSeryjny);
            if (kontener != null)
            {
                try
                {
                    kontenerowce[numerKontenerowca].ZaladujKontener(kontener);
                    kontenery.Remove(kontener);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Blad: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("Nie znaleziono kontenera.");
            }
        }
        else
        {
            Console.WriteLine("Niepoprawny numer kontenerowca.");
        }
    }
}
