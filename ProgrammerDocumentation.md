# Hlavní části programu:

## Http listener

Je v podstatě kopií http listeneru z [oficiální dokumentace Microsoftu](https://learn.microsoft.com/cs-cz/dotnet/api/system.net.httplistener?view=net-9.0) obohacená o Multithreding. Jeho úkolem je poslouchat na předem definované adrese a odpovídat na http dotazy, jejich zpracování je delegováno na `FileReader`.

## FileReader

Podle typu requestu (GET, POST) se rozhodne co dělá, při POST rovnou řeší `SitesHolder` a při GET se rozhoduje podle URL adresy - pokud uživatel požaduje pouze nějaký soubor (obzázek, video, css) tak ho přečte ze souborového systému a vrátí ho. Pokud uživatel požaduje nějakou stránku opět předává request `SitesHolder`.

## SitesHolder

Při startu aplikace se načtou všechny stránky z adresáře `app` a z nich se vytvoří stromová struktura. Jeden vrchol je `SiteNode`, ten má ukazatel na zkompilovanou stránku kterou napsal tvůrce stránky a drží si odkazy na potomky a předka. Pokud uzel není list, tak je v něm uložen případný layout, index se ukládá jako jeho potomek.

Další možnost reprezentace byla pomocí hashovací tabulky, ale pak by se špatně dohledávaly layout rodiče.

Když dostane požadavek na stránku, nejprve se přesvědčí, že existuje (pokud ne, vrátí 404), pak vygeneruje html kód z té poslední stránky a postupuje směrem k rodiči a přidává layouty kolem.

Když dostane POST request, tak opět najde pro kterou stránku je post určen a najde pro kterou jeho komponentu je určen. Tam se data zpracují. Poté vrátí status 303 a url na které se má uživatel přesměrovat (typicky to bude to samé, kde právě je), takže pak user pošle automaticky GET request.

To je protože kduž bych rovnou vrátil stránku, tak se mě Browser zeptá, že bych mohl stratit poslaná data.

## UserSession
Původní mušlenka byla udělat jednu velkou třídu, kam by se ukládali všechny proměnné, ale to mi přišlo neintuitivní a čistě slovník je zase typově nebezpečný.

Takže se vytváří `SessionVar<TVar>`, kterou stačí deklarovat na jeden řádek.

## DefaultHtmlComponent
Asi by bylo lepší, všechny třídy přímo odvozovat od ní, ale byla vytvořena až v průběhu vývoje, takže jsem to nepředělával.

Ale i tak zajišťuje jednodušší opakovatelnost kódu při získávání html a reagování na POST.

## PostUrl
Pořád je potřeba pracovat s tím, jak by měl vypadat POST request, tak je pro to tento struct, aby to bylo co nejjednodušší.



---
# Závěr
Neměl jsem úplně přesně specifikováno, co bych chtěl od programu, jelikož jsem si úplně nebyl jistý, co jde. Přijde mi, ale, že to zase tolik rozšiřování nepotřebuje, až se divím, že se objevuje tak málo chyb.

Na druhou stranu, přijde mi, že co je potřeba napsat vyžaduje strašně moc kódu, je potřeba přeskakovat mezi .cs a html soubory a do komponent. Je to takový odlehčený zkůsob, jak psát malé webové aplikace, ale na nic většího se to moc nehodí.