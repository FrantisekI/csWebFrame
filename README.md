# csWebFrame Dokumentace
František Zápotocký 
I. ročník, letní semestr 2025, NPRG031

Pro programátorskou dokumentaci platí [ProgrammerDocumentation.md](ProgrammerDocumentation.md).

CsWebFrame je určen k vytváření webových aplikací v jazyce C# a html. Základ aplikace se skládá z html souborů, ke kterým jsou odpovídající C# soubory, které mohou generovat dynamický obsah. Proměnné se dosazují do html souborů pomocí formátu `{{promenna}}`.

Projekt zajišťuje, generování výsledných html stránek aplikace a jejich odeslání jako odpovědi na uživatelské requesty.

*Kvůli používaným lomítkům funguje program pouze na linuxových systémech.*

## Instalace
Aplikace funguje s .NET 9.

Naklonujte repozitář csWebFrame pomocí (do této složky pak budete ukládat vaši aplikaci):

```bash
git clone https://github.com/FrantisekI/csWebFrame.git 
```

## Spuštění aplikace
Do souboru `urls.txt`, napište URL adresu, na které chcete, aby aplikace běžela (např. `http://localhost:8080/`).

Aplikaci zkompilujte `dotnet build` a rozběhněte `dotnet run`.

## Vstupy a výstupy
Vstupem od programátora je napsaná webová aplikace podle pravidel csWebFrame (jaká jsou je dále) a uložená do složek app, src a components. 

Výstupem je http listener běžící na předem definované (text v urls.txt) adrese, který odpovídá na http dotazy, typicky od uživatelova prohlížeče.


## Jak by měla vypadat aplikace

Aplikace bude používat složky app, src a components:
- **app** - zde budou umístěny soubory pro konfiguraci aplikace
- **src** - zde bude umístěno vše, co není webová stránka, jako jsou obrázky videa, ikony ... (cesty k nim se udávají bez složky src)
- **components** - zde budou umístěny komponenty, které je možné používat v aplikaci

```
csWebFrame
├── app
|   ├── soubory reprezentující jednotlivé stránky aplikace
|   └── soubory css
├── src
|   └── soubory a média používané v aplikaci
├── components
|   └── objekty pro komponenty, které je možné opakovaně používat 
└── websiteLogic // tady je kód na zpracování requestů a tak, na něj nesahejte
```

### Struktura složky app:
Adresářová struktura v této složce odpovídá struktuře odkazů na stránce, tedy pokud se chcete dostat na stránku `můj.web/zviratka/kotatka/micka` měl by být v této složce soubor `app/zviratka/kotatka/micka.(cs,html)`, nebo `app/zviratka/kotatka/micka/index.(cs,html)` pokud chcete vydět obsah složky `kotatka`, tak byste měli mít v této složce soubor `app/zviratka/kotatka/index.(cs,html)`.

Speciální soubory `index` v url adrese reprezentují adresu složky ve které se nacházejí. Soubory které se jmenují jinak, odpovídají konkrétní stránce, která se nachází na dané adrese.

Soubory `layout` slouží, jako "obal na dceřiné stránky", pokud se nacházejí se uvnitř nějaké složky, tak všechny stránky uvnitř ní budou obaleny tímto layoutem. Obsah této stránky bude vložen na místo proměnné `{{child}}` v layout.html.

Například, pokud chcete, aby všechny stránky se zvířátky měli dole zelenou lištu, tak vytvoříte soubor `app/zviratka/layout.html`.

```html
{{child}}
<div class="footer"></div>
```

Soubory `.cs` jsou dobrovolné, pokud některou stránku chcete udělat jen statickou, tak stačí vytvořit html soubor, ale bez něj se to neobejde, jelikož by C# dosazovali do prázdných stránek.


## Jak psát aplikaci:
Třídu pro generování dynamického obsahu vytvoříte pomocí třídy dovozené od DefaultPage.

Ta musí umět vracet dynamické proměnné z funkce `Render()`, ty se pak budou dosazovat do HTML souboru na místo proměnných v HTML souboru, které jsou ve formátu `{{promenna}}`.

Pokud chcete vrátit dynamický komponent (více dále), vraťte ho jako jeho instanci, jinak vracejte hodnoty tak, aby se daly převést na string.

příklad:
```csharp
public class Index(UserSession s) : DefaultPage(s)
{
    public override Dictionary<string, object> Render()
    {
        int ZviratkoDne = "Pes";
        var komponenta = new ZvireComponent("Kočka");

        return new Dictionary<string, object>
        {
            ["dnesni"] = ZviratkoDne, // jako string
            ["komponenta"] = komponenta, // jako instance komponenty
        };
    }
}
```

## Komponenty
odvozením třídy od `DefaultHtmlComponent` 

```csharp
public class ZvireComponent : DefaultHtmlComponent
{
    public override Dictionary<string, object> GetVariables(UserSession session)
    {
        var obrazek = new ImageComponent($"src/zviratka/{text}.jpg");
        return new Dictionary<string, object>
        {
            // obdobně, jako u Render() u DefaultPage
            ["obrazek"] = obrazek,
        };
    }

    public override string GetHtml(UserSession session, PostUrl postUrl)
    {
        var obrazek = new ImageComponent($"src/zviratka/{text}.jpg");
        return $"<u>{text}</u><br>{obarzek.GetHtml(session, new PostUrl(postUrl, "obrazek"))}";
    }

    // ------- toto je pouze pro náš příklad - funkce můžete přidávat libovolné, pouze předchozí dvě funkce jsou povinné --------
    string text;
    public ZvireComponent(string Jake)
    {
        text = Jake;
    }
}
```

PostUrl bude vysvětleno níže. Pokud nechcete vytvářet html sami, tak můžete použít funkci `CreateFromHtmlFile(UserSession session, PostUrl postUrl)` která si sama zavolá `GetVariables()` a doplní proměnné do html souboru. 

V tom případě uložte do proměnné `HtmlPathFromComponentRoot` cestu relitivně k adresáři `components`, ten se tedy v cestě k html nebude nacházet. tedy něco jako `HtmlPathFromComponentRoot = "zvire.html"`.

Pokud vytváříte html sami, tak je potřeba, abyste PostUrl doplnili vždy o klíč ke komponentě která je uvnitř, je to důležité pro správné fungování POST requestů z tlačítek:

## Tlačítko

Je také odvozeno od `DefaultHtmlComponent`, ale má už naimplementované funkce na vrácení html a má navíc funkci `OnClick()`, která se spustí při kliknutí na tlačítko.

Do té funkce můžete napsat cokoliv, co chcete, aby se stalo při kliknutí na tlačítko.

Tlačítko při stisku odešle POST request ve tvaru zapsaném v `PostUrl`. (Pro zajímavost to je: cesta k právě otevřené stránce `&` index od konce `&` cesta ke komponentě) Tlačítko je v html reprezentováno jako form tag uvnitř kterého jsou input elementy, (Tedy pokud chcete dělat dotazník, tak se to dělá pomocí tačítka.) a alespoň jedno tlačítko s typem submit (Pokud takové není definováno, automaticky se doplní). To vše se ukládá do proměnné `public InputElementAtrributes[]? formElements;` 

Možné atributy tlačítka vnitřního elementu formuláře jsou:
```csharp
public class InputElementAtrributes
{
    public enum PossibleAttributes {input, label, select, option, textarea, button, div}
    public PossibleAttributes attribute;
    public string name;
    public string value;
    public string type;
    public string id;
    public string elementClass;
    public string style;
    public string? labelIsFor; // pouze u tagů typu label
    public string? textInside; 
    public  InputElementAtrributes[]? childs;
// Třída pak pokračuje
}
```

Jak může vypadat tlačítko:
```csharp
public class SendText : Button
{
    private UserSession _session;
    public SendText(UserSession session)
    {
        _session = session;
    }
    public override void OnClick(Dictionary<string, string> data)
    {
        Text text = new Text(_session, "Hello World");
        text.SetFromUserData(data);

    }
}
```


## Ukládání proměnných
Protože se na stránku může dostat více uživatelů, tak se pro každý request vytváří nová instance třídy. Pokud si chcete ukládat nějaká data mezi requesty, můžete si je brát z `UserSession`.

UserSession je v podstatě slovník, kde klíčem je `Type` třídy proměnné a hodnotou je instance této třídy. 

Každý uživatel má svou vlastní UserSession a z nich se vybírá podle cookie, která se posílá s každým requestem. Jednou za 10 minut se nepoužívané UserSession smažou.

Každá instance proměnné od stejné session odpovídá stejné proměnné - to je kvůli tomu, aby při znovu načtení stránky zůstali proměnné stejné, jako byly předtím.

Proměnné si ukládáte pomocí třídy `SessionVar<TVar>`, kde `TVar` je typ proměnné, kterou chcete uložit. 

```csharp
public class Counter(UserSession s, int val) : SessionVar<int>(s, val);
```

Instance se vytváří s parametrem jaká je defaultní hodnota proměnné, pokud není v UserSession ještě uložena.


Navíc má `SessionVar<TVar>` i funkci k načtení dat z POST requestu pomocí `SetFromUserData()`.

Hodnoty se do proměnné ukládají pomocí `Get()` a `Set()`.


---
Měl bych podotknout, že příkladová stránka byla z většiny vygenerováno pomocí jazykového modelu (Claude Sonnet 4), funguje jako příklad aplikace a shrnutí dokumentace, ale správná částe je uložena v .md souborech

