# MinecraftCloneSilk


Silk Minecraft Clone est un projet qui vise à recréer le célèbre jeu Minecraft en utilisant Silk.Net, une bibliothèque C# dédiée à la programmation de jeux vidéo, avec ImGui pour l'interface utilisateur. Le monde est généré de manière procédurale grâce à l'algorithme de Perlin Noise, offrant des paysages variés et uniques. Le jeu dispose également de nombreux menus de débogage pour faciliter le développement et la résolution des problèmes.


## Architecture 

Le code du projet est structuré autour d'un système ECS (Entity-Component-System) pour une organisation plus modulaire et facile à maintenir. Le projet se divise en trois solutions distinctes :

- Benchmark : Cette solution permet de tester différentes approches et d'évaluer leurs performances afin de choisir les plus rapides et efficaces.
- MinecraftCloneSilk : Il s'agit de la solution principale contenant le jeu et les principales fonctionnalités.
- UnitTest : Cette solution comprend des tests unitaires qui permettent de vérifier le bon fonctionnement des différents éléments du jeu, avec ou sans lancer le jeu en arrière-plan.

## Controles 
Voici les commandes de base pour naviguer dans le jeu :

- ZQSD : Déplacement du personnage.
- T : Activer le chat pour entrer des commandes.
- E : Ouvrir l'inventaire.
- F1 : Activer ou désactiver la souris.

## Sauvegarde
Les chunks sont sauvegardés dans le dossier "dossierCourant/Worlds/newWorld". La taille des fichiers est optimisée, mais il y a encore un fichier par chunk. Attention à ne pas créer des mondes trop grands, car cela pourrait générer un grand nombre de fichiers.

## Commandes
Pour accéder à la liste complète des commandes disponibles, tapez /help dans le chat du jeu.

## Crédits
Un grand merci à LearnOpenGL pour les ressources et le soutien apportés au développement de ce projet.

