# csWebFrame Dokumentace

csWebFrame je určen k vytváření webových aplikací v jazyce C#. 

## Instalace
naklonujte repozitář csWebFrame pomocí (do této složky pak budete ukládat vaši aplikaci):

```bash
git clone https://github.com/FrantisekI/csWebFrame.git 
```
do souboru `urls.txt`, napište URL adresu, na které chcete, aby aplikace běžela (např. `http://localhost:5000/`).

## Použití

aplikace bude používat složky app, src a components 
- app - zde budou umístěny soubory pro konfiguraci aplikace
- src - zde budou umístěny soubory a média
- components - zde budou umístěny komponenty, které je možné používat v aplikaci

```
csWebFrame
├── app
|   ├── soubory reprezentující jednotlivé stránky aplikace
|   └── soubory css
├── src
|   └── soubory a média používané v aplikaci
├── components
|   └── objekty pro komponenty, které je možné používat v 
└── websiteLogic // tady je kód na zpracování requestů a tak, na něj nesahejte
```

## Struktura složky app:
ve složce app jeden .cs a/nebo .html soubor reprezentuje jednu stránku aplikace (kromě těch se jménem `layout`, které jsou určeny pro layouty stránek - ten se bude nacházet kolem obsahu stránky)

každá složka reprezentuje jeden string mezi lomítky v URL adrese, takže pokud máte soubor `app/stranka1/stranka2.`, tak se k němu dostanete přes URL `/stranka1/stranka2`, standardne `index` odpovídá odkazu na složku, takže z URL `/stranka1/` se načte `app/stranka1/index.`

Každá stránka musí mít svůj html soubor, který je určen pro její statický obsah, a může mít svůj .cs soubor, který je určen pro dynamický obsah stránky.

## Jak psát aplikaci:
ve složce `app` vytvoříte dynamickou stránku pomocí třídy dovozené od DefaultPage.

ta musí umět vracet dynamické proměnné z funkce `Render()`, ty se pak budou dosazovat do HTML souboru na místo proměnných v HTML souboru, které jsou ve formátu `{{promenna}}`.

Pokud chcete vrátit dynamický komponent (více dále), vraťte ho jako jeho instanci, jinak vracejte hodnoty tak, aby se daly převést na string.

## Komponenty
odvozením třídy od `DefaultHtmlComponent` 

pokud vytváříte html sami, tak je potřeba, abyste PostUrl doplnili vždy o klíč ke komponentě které je uvnitř, je to důležité pro správné fungování POST requestů z tlačítek:

## Tlačítko

Je také odvozeno od `DefaultHtmlComponent`, ale má už naimplementované funkce na vrácení html a má navíc funkci `OnClick()`, která se spustí při kliknutí na tlačítko.

do té funkce můžete napsat cokoliv, co chcete, aby se stalo při kliknutí na tlačítko.

## Ukládání proměnných
Protože se na stránku může dostat více uživatelů se pro každý request vytváří nová instance třídy. Pokud si chcete ukládat nějaká data mezi requesty, můžete si je brát z `UserSession`.

pokud vytvoříte třídu odvozenou od `SessionVar<TVar>`, tak do ní můžete uložit data mezi requesty

navíc má i funkci k načtení dat z POST requestu pomocí `SetFromUserData()`.

