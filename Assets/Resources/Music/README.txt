MUSIC DROP-IN FOLDER
====================

Any audio file you place in this folder (Assets/Resources/Music) becomes
available to the game's AudioManager and the in-game jukebox, looked up by its
file name. The Music mission's "concert hall" jukebox lists whatever it finds
here, so the game works whether or not these files are present.

Recommended Romanian playlist (use legally-owned / royalty-cleared files — they
are NOT bundled with the project for copyright reasons):

  anthem.ogg        Deșteaptă-te, române! (national anthem)
  folk.ogg          a folk song (e.g. Ghiță Munteanu)
  manea.ogg         a manea (e.g. Florin Salam)
  pop.ogg           Romanian pop (e.g. Inna / Alexandra Stan)
  zamfir.ogg        Gheorghe Zamfir — The Lonely Shepherd (pan flute)
  ballad.ogg        an old ballad (e.g. "Nebun de alb", "Un actor grăbit")

Supported formats: .ogg (recommended), .wav, .mp3, .aiff.

The file NAME (without extension) is the lookup key, lower-cased. So a file named
"anthem.ogg" is played with AudioManager.Instance.PlayMusicNamed("anthem").

No files here? The Music mission still teaches instrument recognition using
synthesised tones generated at runtime by AudioManager.PlayTone(...), so nothing
breaks.
