# WPF DAQMx Reader
## Opis
Projekt wykonany na zaliczenie przedmiotu cyfrowe techniki pomiarowe. Program wykrywa podpięte urządzenia 
do komputera a następnie oferuje możliwość wykonania pomiarów i konwersji według danych zakresów pomiarowych.
Dane pomiarowe, które mogą być wczytane lub zmierzone są dalej przetwarzane by określić prędkość i przyśpieszenie.
Użytymi technologiami są biblioteki [National Instruments](https://www.ni.com/pl-pl/support/downloads/drivers/download.ni-daqmx.html#445931),
[Windows Presentation Foundation](https://docs.microsoft.com/pl-pl/visualstudio/designers/getting-started-with-wpf?view=vs-2022)
oraz język [C#](https://docs.microsoft.com/pl-pl/dotnet/csharp/tour-of-csharp/). Oprócz tego warto wspomnieć o bibliotekach 
takich jak [LiveCharts](https://lvcharts.net/) i [ArrayToExcel](https://www.nuget.org/packages/ArrayToExcel/), 
które umożliwiły prezentację oraz zapis danych. 
Wyzwanie stanowiło uruchomienie biblioteki National Instruments, która jest bardzo niekompatybilna
z innymi frameworkami takimi jak [Universal Windows Platform (UWP)](https://docs.microsoft.com/pl-pl/visualstudio/get-started/csharp/tutorial-uwp?view=vs-2022)
czy też [Multi-platform App UI](https://docs.microsoft.com/pl-pl/dotnet/maui/what-is-maui).
Można rozwijać aplikację dalej i wprowadzić multi kanałowy odczyt danych oraz inne. Aplikacja jest w zasadzie 
[dowodem konceptu (Proof of Concept)](https://zajacmarek.com/2020/08/czym-jest-proof-of-concept/).
## Tabela zawartości
- Wykrywanie podłączonych urządzeń do komputera
- Wybór kanału, na którym ma być prowadzony odczyt
- Wybór ustawień kanału
- Wykonanie pomiaru na kanale
- Wyświetlenie wykresów pochodnych pomiaru
- Zapis danych pomiarowych do pliku .xlsx
- Wczytanie danych z pliku .csv
- Wykonanie wykresów pochodnych dla danych wczytanych
- Przesuwanie wykresu, zoomowanie wykresu, wyświetlanie wartości wykresu w konkretnym punkcie
- Możliwość obliczenia wartości przesunięcia na podstawie wprowadzonych zakresów pomiarowych
## Jak uruchomić projekt
1. Należy zainstalować środowisko [Visual Studio 2022 (Community bądź lepsze)](https://visualstudio.microsoft.com/pl/vs/), 
[Visual Studio Code](https://code.visualstudio.com/) z dodatkiem [C#](https://code.visualstudio.com/docs/languages/csharp) 
lub [Jetbrains Rider](https://www.jetbrains.com/rider/)
2. Należy uruchomić środowisko i:
   - sklonować repozytorium za pomocą oprogramowania [git](https://git-scm.com/)
   - sklonować repozytorium za pomocą dostępnych wewnątrz środowiska narzędzi
   - pobrać repozytorium w formie .zip, rozpakować i uruchomić plik .sln
3. Podłączyć urządzenie kompatybilne z DAQMx na przykład USB-6009 użyte na laboratorium bądź je zasymulować za pomocą 
[NI MAX](https://www.ni.com/pl-pl/support/documentation/compatibility/16/measurement---automation-explorer--max--version-installed-with-n.html)
4. Uruchomić projekt za pomocą środowiska
## Jak korzystać z projektu
1. Jeśli urządzenia są podpięte do komputera można rozpocząć wybieranie parametrów i rozpocząć pomiar
2. Po określonym przez użytkownika czasie wcisnąć przycisk Stop
3. Można przeglądać pomiar zoomując i nasuwając kursorem na punkty na wykresie
4. Zapisać dane pomiarowe za pomocą przycisku i wskazać lokalizację
5. Dane pomiarowe znajdują się teraz w pliku .xlsx
## Kolaboratorzy
1. Jerzy Repelowicz
2. Mikołaj Rams
3. Jarosław Morński
4. Damian Mirek
5. Mateusz Nowobilski
## Licencja
Projekt jest licencjonowany przez licencję GNU GPL 3. Postanowienia znajdują się w pliku LICENSE.txt
## Podsumowanie

