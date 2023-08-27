# MinecraftCloneSilk


Ce projet vise à recréer le célèbre jeu Minecraft en utilisant Silk.Net, une bibliothèque C# qui fait la liaison avec OpenGl et ImGui pour l'interface utilisateur. Le monde est généré de manière procédurale grâce à l'algorithme de Perlin Noise, offrant des paysages variés et uniques. Le jeu dispose également de nombreux menus de débogage pour faciliter le développement et la résolution des problèmes.

# Build 
Pour compiler le projet, il dotnet 7.0
utilisez la commande suivantes :
```bash
dotnet publish -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```
vous aurez le dossier "publish" qui contient le l'executable.
vous pouvez supprimer tous les fichiers sauf "MinecraftCloneSilk.exe" 
vous avez besoin de rajouter les fichiers et dossiers suivants :
- le Dossier Assets
- le Dossier Shader
- glfw3.dll ou SDL2.dll (fichiers trouvables dans les dossier de l'export)
- deux dossier Worlds/newWorld




## Architecture 

Le code du projet est structuré autour d'un système ECS (Entity-Component-System) pour une organisation plus modulaire et facile à maintenir. Le projet se divise en trois solutions distinctes :

- Benchmark : Cette solution permet de tester différentes approches et d'évaluer leurs performances afin de choisir les plus rapides et efficaces.
- App : Il s'agit de la solution principale contenant le jeu et les principales fonctionnalités.
- UnitTest : Cette solution comprend des tests unitaires qui permettent de vérifier le bon fonctionnement des différents éléments du jeu, avec ou sans lancer le jeu en arrière-plan.

## Controles 
Voici les commandes de base pour naviguer dans le jeu :

- ZQSD : Déplacement du personnage.
- T : Activer le chat pour entrer des commandes.
- E : Ouvrir l'inventaire.
- F1 : Activer ou désactiver la souris.

## Sauvegarde
Les chunks sont sauvegardés dans le dossier "dossierCourant/Worlds/newWorld". La taille des fichiers est optimisée 
et **NOUVEAU** les chunks sont regroupés dans un seul fichier grâce à LMDB !
Lightning Memory-Mapped Database est une base de données clé-valeur qui permet de stocker des données de manière très efficace.
## Commandes
Pour accéder à la liste complète des commandes disponibles, tapez /help dans le chat du jeu.


# Profiling
En mode debug les méthode avec l'attribut [Timer] sont profilées et les résultats sont enregistrés dans le fichier "dossierCourant/trace.json"
Pour visualiser les résultats, il suffit d'ouvrir le fichier trace.json dans chrome://tracing

## Crédits
Un grand merci à LearnOpenGL





[![Watch the video](https://img.youtube.com/vi/XlYM7xdmK9M/maxresdefault.jpg)](https://youtu.be/XlYM7xdmK9M)

