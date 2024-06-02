# SysProgrProj1
Prvi projekat iz predmeta Sistemsko programiranja

Kreirati Web server koji vrši konverziju slike u gif format. Za proces konverzije se može koristiti
ImageSharp (biblioteku je moguće instalirati korišćenjem NuGet package managera). Gif kreirati
na osnovu iste slike promenom boje za različite frejmove (frejmovi gifa su varijacije slike u drugoj
boji). Osim pomenute, moguće je koristiti i druge biblioteke. Svi zahtevi serveru se šalju preko
browser-a korišćenjem GET metode. U zahtevu se kao parametar navodi naziv fajla, odnosno
slike. Server prihvata zahtev, pretražuje root folder za zahtevani fajl i vrši konverziju. Ukoliko
traženi fajl ne postoji, vratiti grešku korisniku.
