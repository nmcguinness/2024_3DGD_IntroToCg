#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace CubemapBlur
{
    public class CubemapBlurWindow : EditorWindow
    {
        /// <summary>
        /// Rotation encodes the orientation of a cubemap face
        /// (degrees, counter-clockwise rotation).
        /// </summary>
        public enum Rotation
        {
            ROT_0,
            ROT_90,
            ROT_180,
            ROT_270
        }

        /// <summary>
        /// Supported formats for saving the blurred cubemap.
        /// </summary>
        public enum Format
        {
            PNG,
            EXR
        }

        /// <summary>
        /// The Neighbor class defines how neighboring cubemap faces are
        /// oriented to each other.
        /// </summary>
        public class Neighbor
        {
            // neighboring cubemap face
            public readonly CubemapFace cubemapFace;

            // orientation of neighboring cubemap face
            public readonly Rotation rotation;

            /// <summary>
            /// Initializes a new Neighbor instance with a cubemap face and a
            /// rotation.
            /// </summary>
            /// <param name="cubemapFace">Cubemap face.</param>
            /// <param name="rotation">Rotation.</param>
            public Neighbor(CubemapFace cubemapFace, Rotation rotation)
            {
                this.cubemapFace = cubemapFace;
                this.rotation = rotation;
            }
        }

        /// <summary>
        /// The MTCubemap is a custom cubemap that can be accessed from outside
        /// Unity's main thread.
        /// </summary>
        public class MTCubemap
        {
            // the side length the cubemap
            public readonly int width;

            // the six cubemap faces
            private readonly Dictionary<CubemapFace, Color[]> faces = new Dictionary<CubemapFace, Color[]>();

            /// <summary>
            /// Initializes a new MTCubemap instance from a given cubemap.
            /// </summary>
            /// <param name="cubemap">The original cubemap.</param>
            public MTCubemap(Cubemap cubemap)
            {
                width = cubemap.width;
                foreach (CubemapFace cubemapFace in ALL_CUBEMAP_FACES)
                {
                    Color[] colors = cubemap.GetPixels(cubemapFace);
                    faces.Add(cubemapFace, colors);
                }
            }

            /// <summary>
            /// Initializes an empty MTCubemap instance with a given width.
            /// </summary>
            /// <param name="width">Width of one cubemap face.</param>
            public MTCubemap(int width)
            {
                this.width = width;
                foreach (CubemapFace cubemapFace in ALL_CUBEMAP_FACES)
                {
                    Color[] colors = new Color[width * width];
                    faces.Add(cubemapFace, colors);
                }
            }

            /// <summary>
            /// Gets the color of a single pixel.
            /// </summary>
            /// <returns>The pixel.</returns>
            /// <param name="cubemapFace">Cubemap face.</param>
            /// <param name="x">The x coordinate.</param>
            /// <param name="y">The y coordinate.</param>
            public Color GetPixel(CubemapFace cubemapFace, int x, int y)
            {
                return faces[cubemapFace][y * width + x];
            }

            /// <summary>
            /// Sets the color of a single pixel.
            /// </summary>
            /// <param name="cubemapFace">Cubemap face.</param>
            /// <param name="x">The x coordinate.</param>
            /// <param name="y">The y coordinate.</param>
            /// <param name="color">Color.</param>
            public void SetPixel(CubemapFace cubemapFace, int x, int y, Color color)
            {
                faces[cubemapFace][y * width + x] = color;
            }

            /// <summary>
            /// Gets all pixels of the given cubemap face.
            /// </summary>
            /// <returns>The pixel colors.</returns>
            /// <param name="cubemapFace">Cubemap face.</param>
            public Color[] GetPixels(CubemapFace cubemapFace)
            {
                return faces[cubemapFace];
            }

            /// <summary>
            /// Sets all pixels of the given cubemap face.
            /// </summary>
            /// <param name="colors">Colors.</param>
            /// <param name="cubemapFace">Cubemap face.</param>
            public void SetPixels(Color[] colors, CubemapFace cubemapFace)
            {
                faces.Remove(cubemapFace);
                faces.Add(cubemapFace, colors);
            }

            /// <summary>
            /// Creates a cubemap texture with the given texture format.
            /// </summary>
            /// <returns>The cubemap.</returns>
            /// <param name="textureFormat">Texture format.</param>
            /// <param name="mipChain">Should mipmaps be created?.
            /// See <see cref="UnityEngine.Cubemap"/> constructor.</param>
            public Cubemap ToCubemap(TextureFormat textureFormat, bool mipChain)
            {
                Cubemap cubemap = new Cubemap(width, textureFormat, mipChain);
                foreach (CubemapFace cubemapFace in ALL_CUBEMAP_FACES)
                    cubemap.SetPixels(faces[cubemapFace], cubemapFace);
                cubemap.Apply();
                return cubemap;
            }
        }

        // define how neighboring faces are oriented to each other
        private static readonly Dictionary<CubemapFace, Neighbor> leftNeighbor = new Dictionary<CubemapFace, Neighbor>()
        {
            {CubemapFace.PositiveZ, new Neighbor(CubemapFace.NegativeX, Rotation.ROT_0)},
            {CubemapFace.NegativeX, new Neighbor(CubemapFace.NegativeZ, Rotation.ROT_0)},
            {CubemapFace.NegativeZ, new Neighbor(CubemapFace.PositiveX, Rotation.ROT_0)},
            {CubemapFace.PositiveX, new Neighbor(CubemapFace.PositiveZ, Rotation.ROT_0)},
            {CubemapFace.PositiveY, new Neighbor(CubemapFace.NegativeX, Rotation.ROT_270)},
            {CubemapFace.NegativeY, new Neighbor(CubemapFace.NegativeX, Rotation.ROT_90)}
        };

        private static readonly Dictionary<CubemapFace, Neighbor> rightNeighbor = new Dictionary<CubemapFace, Neighbor>()
        {
            {CubemapFace.PositiveZ, new Neighbor(CubemapFace.PositiveX, Rotation.ROT_0)},
            {CubemapFace.PositiveX, new Neighbor(CubemapFace.NegativeZ, Rotation.ROT_0)},
            {CubemapFace.NegativeZ, new Neighbor(CubemapFace.NegativeX, Rotation.ROT_0)},
            {CubemapFace.NegativeX, new Neighbor(CubemapFace.PositiveZ, Rotation.ROT_0)},
            {CubemapFace.PositiveY, new Neighbor(CubemapFace.PositiveX, Rotation.ROT_90)},
            {CubemapFace.NegativeY, new Neighbor(CubemapFace.PositiveX, Rotation.ROT_270)}
        };

        private static readonly Dictionary<CubemapFace, Neighbor> topNeighbor = new Dictionary<CubemapFace, Neighbor>()
        {
            {CubemapFace.PositiveZ, new Neighbor(CubemapFace.PositiveY, Rotation.ROT_0)},
            {CubemapFace.NegativeX, new Neighbor(CubemapFace.PositiveY, Rotation.ROT_90)},
            {CubemapFace.NegativeZ, new Neighbor(CubemapFace.PositiveY, Rotation.ROT_180)},
            {CubemapFace.PositiveX, new Neighbor(CubemapFace.PositiveY, Rotation.ROT_270)},
            {CubemapFace.PositiveY, new Neighbor(CubemapFace.NegativeZ, Rotation.ROT_180)},
            {CubemapFace.NegativeY, new Neighbor(CubemapFace.PositiveZ, Rotation.ROT_0)}
        };

        private static readonly Dictionary<CubemapFace, Neighbor> bottomNeighbor = new Dictionary<CubemapFace, Neighbor>()
        {
            {CubemapFace.PositiveZ, new Neighbor(CubemapFace.NegativeY, Rotation.ROT_0)},
            {CubemapFace.NegativeX, new Neighbor(CubemapFace.NegativeY, Rotation.ROT_270)},
            {CubemapFace.NegativeZ, new Neighbor(CubemapFace.NegativeY, Rotation.ROT_180)},
            {CubemapFace.PositiveX, new Neighbor(CubemapFace.NegativeY, Rotation.ROT_90)},
            {CubemapFace.PositiveY, new Neighbor(CubemapFace.PositiveZ, Rotation.ROT_0)},
            {CubemapFace.NegativeY, new Neighbor(CubemapFace.NegativeZ, Rotation.ROT_180)}
        };

        // list of all six cubemap faces
        private static readonly List<CubemapFace> ALL_CUBEMAP_FACES = new List<CubemapFace> {CubemapFace.NegativeX, CubemapFace.PositiveX, CubemapFace.NegativeY, CubemapFace.PositiveY, CubemapFace.NegativeZ, CubemapFace.PositiveZ};

        // sigma for gaussian blur
        private float sigma = 1;

        // number of times the blur process is being repeated
        private int passes = 3;

        // process each face in a separate thread
        private readonly bool multiThreading = true;

        // the original cubemap
        private Cubemap original;

        // empty cubemap used for preview when no cubemap is available
        private Cubemap emptyCubemap;

        // target cubemap after bluring process
        private Cubemap target;

        // multi-threading compatible cubemap used for processing
        private MTCubemap mtTarget;

        // error message
        private string errorMessage = null;

        // preview editor displaying the original / blurred cubemap
        private Editor previewEditor;

        // main thread running the blur process
        private Thread blurThread = null;

        // lock object used to synchronize threads
        private readonly object threadLockObject = new object();

        // number of pixels processed so far
        private int pixelsProcessed = 0;

        // indicates whether the result is available
        private bool done = false;

        /// <summary>
        /// Shows the Cubemap Blur window.
        /// </summary>
        [MenuItem("Tools/Cubemap Blur")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(CubemapBlurWindow), false);
            GUIContent titleContent = EditorGUIUtility.IconContent("PreMatCube");
            titleContent.text = "Cubemap Blur";
            window.titleContent = titleContent;
            window.minSize = new Vector2(290, 475);
        }

        // update executed multiple times per second
        private void Update()
        {
            // check if new blurred cubemap is ready
            lock (threadLockObject)
            {
                if (done)
                {
                    // convert to target cubemap when blur process is complete
                    target = mtTarget.ToCubemap(TextureFormat.RGBA32, false);

                    // destroy preview
                    if (previewEditor != null)
                    {
                        DestroyImmediate(previewEditor);
                        previewEditor = null;
                    }

                    done = false;
                }
            }

            // repaint GUI so it gets updated while the blur thread is running
            Repaint();
        }

        // draw the GUI
        private void OnGUI()
        {
            GUILayout.Label("Gaussian Blur", EditorStyles.boldLabel);

            lock (threadLockObject)
            {
                // disable settings while blur thread is running
                EditorGUI.BeginDisabledGroup(blurThread != null);

                // cubemap
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Cubemap", "Cubemap to be blurred"), new GUILayoutOption[] {GUILayout.Width(80)});
                Cubemap oldOriginal = original;
                original = (Cubemap) EditorGUILayout.ObjectField(original, typeof(Cubemap), false);
                if (original != oldOriginal && previewEditor != null)
                {
                    // reset preview, target, progress and error message
                    DestroyImmediate(previewEditor);
                    previewEditor = null;
                    target = null;
                    pixelsProcessed = 0;

                    errorMessage = null;
                }

                EditorGUILayout.EndHorizontal();

                // sigma
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Sigma", "Sigma (standard deviation) used for Gaussian filtering"), new GUILayoutOption[] {GUILayout.Width(80)});
                sigma = EditorGUILayout.Slider(sigma, 0.1f, 16f);
                EditorGUILayout.EndHorizontal();

                // passes
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Passes", "Number of times the filter is applied"), new GUILayoutOption[] {GUILayout.Width(80)});
                passes = EditorGUILayout.IntSlider(passes, 1, 64);
                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();
            }

            // cubemap preview
            EditorGUILayout.Space();
            GUIStyle backgroundColor = new GUIStyle();
            backgroundColor.normal.background = Texture2D.blackTexture;
            if (previewEditor == null)
            {
                if (target != null)
                    previewEditor = Editor.CreateEditor(target);
                else if (original != null)
                    previewEditor = Editor.CreateEditor(original);
                else
                {
                    if (emptyCubemap == null)
                    {
                        emptyCubemap = new Cubemap(1, TextureFormat.RGBA32, false);
                        foreach (CubemapFace cubemapFace in ALL_CUBEMAP_FACES)
                            emptyCubemap.SetPixel(cubemapFace, 0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
                        emptyCubemap.Apply();
                    }

                    previewEditor = Editor.CreateEditor(emptyCubemap);
                }
            }

            previewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(290, 290), backgroundColor);

            lock (threadLockObject)
            {
                // error message
                if (!string.IsNullOrEmpty(errorMessage))
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                // blur / abort button
                if (blurThread == null)
                {
                    EditorGUI.BeginDisabledGroup(original == null);
                    if (GUILayout.Button("Blur", new GUILayoutOption[] {GUILayout.Height(30)}) && original != null)
                    {
                        errorMessage = null;
                        StartBlurThread(original);
                        if (previewEditor != null)
                        {
                            // destroy preview
                            DestroyImmediate(previewEditor);
                            previewEditor = null;
                        }
                    }

                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    if (GUILayout.Button("Abort", new GUILayoutOption[] {GUILayout.Height(30)}) && original != null)
                    {
                        errorMessage = null;
                        StopBlurThread();
                    }
                }

                // save button
                EditorGUI.BeginDisabledGroup(target == null || blurThread != null);
                if (GUILayout.Button(EditorGUIUtility.IconContent("SaveActive"), new GUILayoutOption[] {GUILayout.Width(30), GUILayout.Height(30)}))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Save as PNG"), false, SaveDialog, Format.PNG);
                    menu.AddItem(new GUIContent("Save as EXR"), false, SaveDialog, Format.EXR);
                    menu.ShowAsContext();
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }

            // progress bar
            lock (threadLockObject)
            {
                if (blurThread != null && original != null)
                {
                    int totalPixels = original.width * original.width * 12 * passes;
                    float progress = (float) pixelsProcessed / totalPixels;
                    GUILayout.FlexibleSpace();
                    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(256, 20), progress, Mathf.FloorToInt(progress * 100f) + "%");
                }
            }
        }

        private void SaveDialog(object obj)
        {
            Format format = (Format) obj;
            switch (format)
            {
                case Format.PNG:
                    SaveCubemap(target, Format.PNG, EditorUtility.SaveFilePanel("Save cubemap as PNG texture", "", original.name + ".png", "png"));
                    break;
                case Format.EXR:
                    SaveCubemap(target, Format.EXR, EditorUtility.SaveFilePanel("Save cubemap as EXR texture", "", original.name + ".exr", "exr"));
                    break;
            }
        }

        // start a thread filtering the given cubemap
        private void StartBlurThread(Cubemap cubemap)
        {
            lock (threadLockObject)
            {
                // can only process one cubemap at a time
                if (blurThread != null)
                    throw new UnityException("Thread is already running.");

                try
                {
                    // create MTCubemap from cubemap
                    MTCubemap mtCubemap = new MTCubemap(cubemap);

                    // create and start blur thread
                    blurThread = new Thread(() =>
                    {
                        try
                        {
                            // blur cubemap
                            mtTarget = Blur(mtCubemap);
                            lock (threadLockObject)
                            {
                                done = true;
                                blurThread = null;
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            Debug.LogWarning("Thread was aborted.");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    });
                    blurThread.Start();
                }
                catch (UnityException e)
                {
                    errorMessage = e.Message;
                    Debug.LogError(e);
                }
            }
        }

        // stop current blur thread
        private void StopBlurThread()
        {
            lock (threadLockObject)
            {
                if (blurThread == null)
                    return;

                blurThread.Abort();
                blurThread = null;
                pixelsProcessed = 0;
            }
        }

        // process gaussian filtering of given cubemap
        private MTCubemap Blur(MTCubemap cubemap)
        {
            lock (threadLockObject)
            {
                pixelsProcessed = 0;
            }

            // increase kernel width and height towards the edges of each face
            // to account for angular distortion 
            float correction = Mathf.Sqrt(2) - 1f;
            float correctionExp = 1f;

            if (sigma < 0.1f)
                throw new UnityException("sigma must be at least 0.1");

            // get cubemap width
            int width = cubemap.width;

            // create temp and target cubemaps
            MTCubemap temp = new MTCubemap(width);
            MTCubemap result = new MTCubemap(width);

            // copy original target
            foreach (CubemapFace cubemapFace in ALL_CUBEMAP_FACES)
                result.SetPixels(cubemap.GetPixels(cubemapFace), cubemapFace);

            // apply blur in multiple passes
            for (int p = 0; p < passes; p++)
            {
                // blur horizontally
                List<Thread> horizontalBlurThreads = new List<Thread>();
                foreach (CubemapFace cubemapFace in ALL_CUBEMAP_FACES)
                {
                    // create one thread for each face
                    Thread horizontalBlurThread = new Thread(() =>
                    {
                        for (int y = 0; y < width; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                // correct sigma based on offset
                                float multOffset = 1f + Mathf.Pow(Mathf.Abs(x - width / 2f) / (width / 2f), correctionExp) * correction;

                                // calculate corrected sigma and kernel width
                                float correctedSigma = sigma * multOffset;
                                int kernelWidth = Mathf.CeilToInt(6f * correctedSigma);

                                // filter color
                                float weightSum = 0f;
                                float r = 0f;
                                float g = 0f;
                                float b = 0f;
                                float a = 0f;
                                for (int i = 0; i < kernelWidth; i++)
                                {
                                    int offset = i - Mathf.CeilToInt(kernelWidth / 2);
                                    float weight = Mathf.Exp(-(offset * offset / (2 * correctedSigma * correctedSigma)));
                                    Color tmpColor = GetCubemapColor(result, cubemapFace, x + offset, y);
                                    r += tmpColor.r * weight;
                                    g += tmpColor.g * weight;
                                    b += tmpColor.b * weight;
                                    a += tmpColor.a * weight;
                                    weightSum += weight;
                                }

                                // normalize
                                r /= weightSum;
                                g /= weightSum;
                                b /= weightSum;
                                a /= weightSum;
                                temp.SetPixel(cubemapFace, x, y, new Color(r, g, b, a));

                                // update progress
                                lock (threadLockObject)
                                    pixelsProcessed++;
                            }
                        }
                    });
                    horizontalBlurThreads.Add(horizontalBlurThread);
                    horizontalBlurThread.Start();
                    if (!multiThreading)
                        horizontalBlurThread.Join();
                }

                // wait for all horizontal blur threads to complete
                if (multiThreading)
                {
                    foreach (Thread horizontalBlurThread in horizontalBlurThreads)
                        horizontalBlurThread.Join();
                }

                // blur vertically
                List<Thread> verticalBlurThreads = new List<Thread>();
                foreach (CubemapFace cubemapFace in ALL_CUBEMAP_FACES)
                {
                    // create one thread for each face
                    Thread verticalBlurThread = new Thread(() =>
                    {
                        for (int y = 0; y < width; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                // correct sigma based on offset
                                float multOffset = 1f + Mathf.Pow(Mathf.Abs(y - width / 2f) / (width / 2f), correctionExp) * correction;

                                // calculate corrected sigma and kernel height
                                float correctedSigma = sigma * multOffset;
                                int kernelHeight = Mathf.CeilToInt(6f * correctedSigma);

                                // filter color
                                float weightSum = 0f;
                                float r = 0f;
                                float g = 0f;
                                float b = 0f;
                                float a = 0f;
                                for (int i = 0; i < kernelHeight; i++)
                                {
                                    int offset = i - Mathf.CeilToInt(kernelHeight / 2);
                                    float weight = Mathf.Exp(-(offset * offset / (2 * correctedSigma * correctedSigma)));
                                    Color tmpColor = GetCubemapColor(temp, cubemapFace, x, y + offset);
                                    r += tmpColor.r * weight;
                                    g += tmpColor.g * weight;
                                    b += tmpColor.b * weight;
                                    a += tmpColor.a * weight;
                                    weightSum += weight;
                                }

                                // normalize
                                r /= weightSum;
                                g /= weightSum;
                                b /= weightSum;
                                a /= weightSum;
                                result.SetPixel(cubemapFace, x, y, new Color(r, g, b, a));

                                // update progress
                                lock (threadLockObject)
                                    pixelsProcessed++;
                            }
                        }
                    });
                    verticalBlurThreads.Add(verticalBlurThread);
                    verticalBlurThread.Start();
                    if (!multiThreading)
                        verticalBlurThread.Join();
                }

                // wait for all vertical blur threads to complete
                if (multiThreading)
                {
                    foreach (Thread verticalBlurThread in verticalBlurThreads)
                    {
                        verticalBlurThread.Join();
                    }
                }
            }

            return result;
        }

        // gets color of a pixel single pixel on the specified cubemap face and the
        // specified coordinates. If the coordinates are less than 0 or greater than
        // the width of the cubemap, the color will be taken from the respective
        // neighboring face.
        private static Color GetCubemapColor(MTCubemap cubemap, CubemapFace cubemapFace, int x, int y)
        {
            // at least one of the two coordinates needs to be within the range of
            // [0, cubemap.width - 1]
            if ((x < 0 || x >= cubemap.width) && (y < 0 || y >= cubemap.width))
                throw new UnityException("Cannot determine color if both coordinates are out of bounds.");

            // determine which neighbor contains the requested pixel
            Neighbor neighbor = null;
            CubemapFace tmpCubemapFace = cubemapFace;
            while (x < 0)
            {
                neighbor = leftNeighbor[tmpCubemapFace];
                tmpCubemapFace = neighbor.cubemapFace;
                x += cubemap.width;
            }

            while (x >= cubemap.width)
            {
                neighbor = rightNeighbor[tmpCubemapFace];
                tmpCubemapFace = neighbor.cubemapFace;
                x -= cubemap.width;
            }

            while (y < 0)
            {
                neighbor = topNeighbor[tmpCubemapFace];
                tmpCubemapFace = neighbor.cubemapFace;
                y += cubemap.width;
            }

            while (y >= cubemap.width)
            {
                neighbor = bottomNeighbor[tmpCubemapFace];
                tmpCubemapFace = neighbor.cubemapFace;
                y -= cubemap.width;
            }

            // pixel is in this cubemap face
            if (neighbor == null)
                return cubemap.GetPixel(cubemapFace, x, y);

            // get pixel from neighboring cubemap face
            switch (neighbor.rotation)
            {
                case Rotation.ROT_0:
                    return cubemap.GetPixel(neighbor.cubemapFace, x, y);
                case Rotation.ROT_90:
                    return cubemap.GetPixel(neighbor.cubemapFace, cubemap.width - 1 - y, x);
                case Rotation.ROT_180:
                    return cubemap.GetPixel(neighbor.cubemapFace, cubemap.width - 1 - x, cubemap.width - 1 - y);
                case Rotation.ROT_270:
                    return cubemap.GetPixel(neighbor.cubemapFace, y, cubemap.width - 1 - x);
                default:
                    throw new UnityException("Invalid rotation");
            }
        }

        // save cubemap to file
        private void SaveCubemap(Cubemap cubemap, Format format, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            // store cubemap in texture2d
            Texture2D tex = new Texture2D(cubemap.width * 6, cubemap.width, format == Format.EXR ? TextureFormat.RGBAFloat : TextureFormat.RGBA32, false);
            tex.SetPixels(0 * cubemap.width, 0, cubemap.width, cubemap.width, cubemap.GetPixels(CubemapFace.PositiveX));
            tex.SetPixels(1 * cubemap.width, 0, cubemap.width, cubemap.width, cubemap.GetPixels(CubemapFace.NegativeX));
            tex.SetPixels(2 * cubemap.width, 0, cubemap.width, cubemap.width, cubemap.GetPixels(CubemapFace.PositiveY));
            tex.SetPixels(3 * cubemap.width, 0, cubemap.width, cubemap.width, cubemap.GetPixels(CubemapFace.NegativeY));
            tex.SetPixels(4 * cubemap.width, 0, cubemap.width, cubemap.width, cubemap.GetPixels(CubemapFace.PositiveZ));
            tex.SetPixels(5 * cubemap.width, 0, cubemap.width, cubemap.width, cubemap.GetPixels(CubemapFace.NegativeZ));

            // flip vertically
            Color[] texColors = tex.GetPixels();
            for (int y = 0; y < cubemap.width; y++)
                Array.Reverse(texColors, y * cubemap.width * 6, cubemap.width * 6);
            Array.Reverse(texColors);
            tex.SetPixels(texColors);

            // convert to linear color space
            if (format == Format.EXR)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {
                        Color c = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, c.linear);
                    }
                }
            }

            // write to file
            byte[] bytes;
            switch (format)
            {
                case Format.PNG:
                    bytes = tex.EncodeToPNG();
                    break;
                case Format.EXR:
                    bytes = tex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
                    break;
                default:
                    throw new UnityException("Invalid format: " + format.ToString());
            }

            File.WriteAllBytes(path, bytes);
        }
    }
}

#endif