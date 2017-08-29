using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ImageProcessor;
using log4net;
using Newtonsoft.Json;

namespace AnimateSpriteSheetSlicer {
    internal class ConsoleSlicer {
        private static readonly ILog Log = LogManager.GetLogger("ConsoleSlicer");

        private readonly string[] _args;

        private ConsoleSlicer(string[] args) {
            _args = args;
        }

        public static void Run(string[] args) {
            ConsoleSlicer consoleSlicer = new ConsoleSlicer(args);
            consoleSlicer.Run();
        }

        private void Run() {
            if (_args.Length == 0) {
                Log.Info("Usage: AnimateSpriteSheetSlicer <JSON-Array file path> <output directory>");
                Environment.ExitCode = 1;
                return;
            }

            string jsonFilePath = _args[0];

            if (!File.Exists(jsonFilePath)) {
                Log.FatalFormat("JSON file '{0}' not found", jsonFilePath);
                Environment.ExitCode = 1;
                return;
            }
            Log.InfoFormat("Reading JSON file '{0}'", jsonFilePath);

            string jsonFile = File.ReadAllText(jsonFilePath);
            dynamic spriteSheetData = JsonConvert.DeserializeObject(jsonFile);

            string spriteSheetImageFileName = spriteSheetData["meta"]["image"];
            dynamic spriteSheetFramesData = spriteSheetData["frames"];

            List<FrameData> spriteSheetFrames = new List<FrameData>();
            foreach (dynamic spriteSheetFrameData in spriteSheetFramesData) {
                string name = spriteSheetFrameData["filename"];
                // Clean up the name a bit
                int tmp = name.IndexOf(" instance");
                if (tmp != -1) {
                    name = name.Substring(0, tmp);
                }
                name = name.Trim();
                spriteSheetFrames.Add(
                    new FrameData(
                        name,
                        new Rectangle(
                            (int) spriteSheetFrameData["frame"]["x"],
                            (int) spriteSheetFrameData["frame"]["y"],
                            (int) spriteSheetFrameData["frame"]["w"],
                            (int) spriteSheetFrameData["frame"]["h"]
                        )
                    ));
            }

            Log.InfoFormat("Found {0} frames", spriteSheetFrames.Count);

            Log.InfoFormat("Reading image file '{0}'", spriteSheetImageFileName);

            string jsonFileDirectory = Path.GetDirectoryName(jsonFilePath);
            spriteSheetImageFileName = Path.Combine(jsonFileDirectory, spriteSheetImageFileName);

            string jsonFileName = Path.GetFileNameWithoutExtension(jsonFilePath);
            string outputDirectory = _args.Length >= 2 ? _args[1] : Path.Combine(jsonFileDirectory, jsonFileName);
            Directory.CreateDirectory(outputDirectory);

            if (!File.Exists(spriteSheetImageFileName)) {
                Log.FatalFormat("Image file '{0}' not found", spriteSheetImageFileName);
                Environment.ExitCode = 1;
                return;
            }
            byte[] spriteSheetImageBytes = File.ReadAllBytes(spriteSheetImageFileName);
            using (MemoryStream inStream = new MemoryStream(spriteSheetImageBytes)) {
                using (ImageFactory imageFactory = new ImageFactory()) {
                    using (ImageFactory spriteSheetImageSource = imageFactory.Load(inStream)) {
                        using (Image clonedSpriteSheetImage = (Image) imageFactory.Image.Clone()) {
                            for (int i = 0; i < spriteSheetFrames.Count; i++) {
                                FrameData spriteSheetFrame = spriteSheetFrames[i];
                                using (ImageFactory spriteSheetImage = new ImageFactory().Load(clonedSpriteSheetImage)) {
                                    using (ImageFactory frameImage = spriteSheetImage.Crop(spriteSheetFrame.Rect)) {
                                        string frameFilePath =
                                            Path.Combine(
                                                outputDirectory,
                                                spriteSheetFrame.Name + "." + spriteSheetImageSource.CurrentImageFormat.DefaultExtension
                                                );
                                        Log.InfoFormat("Saving frame file {0}/{1} ('{2}')", i + 1, spriteSheetFrames.Count, spriteSheetFrame.Name);

                                        frameImage
                                            .Format(spriteSheetImageSource.CurrentImageFormat)
                                            .Save(frameFilePath);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private struct FrameData {
            public readonly string Name;
            public readonly Rectangle Rect;

            public FrameData(string name, Rectangle rect) {
                Name = name;
                Rect = rect;
            }
        }
    }
}
