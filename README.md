    Applikation med Azure AI Services ( Vision ) Bildtjänster i Azure AI

    Användaren får skicka in en bild via URL som ska analyseras, sparas i string imageUrl,
    när den sparats får användaren välja vilken storlek en miniatyrbild sedan ska sparas i (höjd och bredd). 
    Efter det får användaren välja var minityrbilden ska sparas
    på Skrivbordet, i Aktuell Arbetskatalog eller hårdkodad väg i mitt repo(byt ut den mot din sökväg till ditt repo).
    Bilden Analyseras via Azure AI services (cognitive service) som innefattar även Computer Vision.
    Användaren får en beskrivning av vad bilden visar samt relevanta taggar.
    Bilden sparas sedan som en Thumbnail/miniatyrbild på vald plats.
    Programmet körs tills användaren skriver in 'exit'.

    Tjänsterna i Azure är avstängda, så skapa en ny tjänst och fyll i din egen Endpoint och Nyckel ifrån Azure i appsetting.json
