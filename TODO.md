co bych měl udělat, aby mi fungoval C# webový framework tak, jak si představuji:

1.	HttpListener ####

když requestne něco končící /, tak pustí ten .cs v té složce možnosti:
    - spustí kód ve složce a jako parametr mu dá přečtená index.html soubor, ten změní ty parametry v {{}} a vrátí to 
    - spustí ten kód - ten si musí nějak držet v jakém je stavu a pak vrátí třeba json který pak přečte filereader a vymění je v tom souboru, ten pak pošle ke klientovi.

3.	Stav - zařídit, abych mohl do html napsat proměnnou a ono to pak bylo před posláním klientovy změněno na hodnotu té proměnné někde v programu
- když to bude chtít složku, nebo html, tak se podívat do stejné složky, jestli se v ní nenachází .cs soubor se stejným jménem, v něm přečíst proměnnou a tu pak zobrazit


2.	Reaktivita - zařídit, aby když uživatel zmáčkne tlačítko, tak aby to server nějak zaregistroval, spustil funkci a vrátil výsledek

4.	