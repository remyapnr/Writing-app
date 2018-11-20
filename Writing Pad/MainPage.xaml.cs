using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Writing_Pad.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Writing_Pad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string allNotes = string.Empty;
        string dynamicNotes = string.Empty;
        List<string> HandWrittenNotesList = null;
        HandWritingRecognitionVM vm = new HandWritingRecognitionVM();

        public MainPage()
        {
            this.InitializeComponent();

            // Set supported inking device types.
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;
            LoadDataBaseAsync();
            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private async void LoadDataBaseAsync()
        {
            #region Initialise local storage
            var dbFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("QA.sqlite") as StorageFile;
            var localFolder = ApplicationData.Current.LocalFolder;
            if (dbFile == null)
            {

                // first time ... copy the .db file from assets to local  folder
                var originalDbFileUri = new Uri("ms-appx:///Assets/DB/QA.sqlite");
                var originalDbFile = await StorageFile.GetFileFromApplicationUriAsync(originalDbFileUri);

                dbFile = await originalDbFile.CopyAsync(localFolder);

            }
            else
            {




            }

            #endregion
        }
        private async void btnRecognize_Click(object sender, RoutedEventArgs e)
        {
            // Get all strokes on the InkCanvas.
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            // Ensure an ink stroke is present.
            if (currentStrokes.Count > 0)
            {
                // Create a manager for the InkRecognizer object
                // used in handwriting recognition.
                InkRecognizerContainer inkRecognizerContainer =
                   new InkRecognizerContainer();

                // inkRecognizerContainer is null if a recognition engine is not available.
                if (!(inkRecognizerContainer == null))
                {
                    // Recognize all ink strokes on the ink canvas.
                    IReadOnlyList<InkRecognitionResult> recognitionResults =
                        await inkRecognizerContainer.RecognizeAsync(
                            inkCanvas.InkPresenter.StrokeContainer,
                            InkRecognitionTarget.All);
                    // Process and display the recognition results.
                    if (recognitionResults.Count > 0)
                    {
                        string str = string.Empty;
                        // Iterate through the recognition results.
                        foreach (var result in recognitionResults)
                        {
                            // Get all recognition candidates from each recognition result.
                            IReadOnlyList<string> candidates = result.GetTextCandidates();
                            //str += "Candidates: " + candidates.Count.ToString() + "\n";
                            str = candidates.FirstOrDefault();
                        }
                        // Display the recognition candidates.
                        //txtrecognitionResult.Text = str;
                        txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, string.IsNullOrEmpty(str) ? string.Empty : str);
                        allNotes += str;
                        //// Clear the ink canvas once recognition is complete.
                        //inkCanvas.InkPresenter.StrokeContainer.Clear();
                    }
                    else
                    {
                       
                        txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, "No recognition results.");
                    }
                }
                else
                {
                    Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog("You must install handwriting recognition engine.");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
               txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, "No ink strokes to recognize.");
            }
        }

        private void StoreCanvas(string base64CanvasData)
        {
            if (HandWrittenNotesList == null)
            {
                HandWrittenNotesList = new List<string>();
            }
            HandWrittenNotesList.Add(base64CanvasData);
        }
        private async void LoadCanvasListView(string base64string)
        {
            if (!string.IsNullOrEmpty(base64string))
            {
                var bitmap = new BitmapImage();
                var handwrittenimage = System.Convert.FromBase64String(base64string);

                MemoryStream ms = new MemoryStream(handwrittenimage, 0, handwrittenimage.Length);
                var randStream = await ConvertToRandomAccessStream(ms);


                using (var stream = await randStream.OpenReadAsync())
                {
                    await bitmap.SetSourceAsync(stream);
                }
                lstHandWrittenNotes.Items.Add(bitmap);
            }
        }
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string recognizedText;
            txtrecognitionResult.Document.GetText(Windows.UI.Text.TextGetOptions.UseCrlf, out recognizedText);
            if (!string.IsNullOrEmpty(recognizedText))
            {
                vm.SaveRecognizedNotes(recognizedText);
            }
           
            string str = string.Empty;
            txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, string.IsNullOrEmpty(str) ? string.Empty : str);
            // Get all strokes on the InkCanvas.
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (currentStrokes.Count > 0)
            {
                var note = await GetByteArray(inkCanvas);
                var notebase64String = Convert.ToBase64String(note);
                StoreCanvas(notebase64String);
                vm.SaveHandwritingNotes(notebase64String);
                LoadCanvasListView(notebase64String);

                inkCanvas.InkPresenter.StrokeContainer.Clear();
            }

        }

        static public async Task<byte[]> GetByteArray(InkCanvas CardInkCanvas)
        {
            MemoryStream ms = new MemoryStream();

            // Write the ink strokes to the output stream.
            using (IOutputStream outputStream = ms.AsOutputStream())
            {
                //Add await before async functions so that the async functions get executed before return.
                await CardInkCanvas.InkPresenter.StrokeContainer.SaveAsync(ms.AsOutputStream());
                await outputStream.FlushAsync();
            }

            return ms.ToArray();
        }
        private static async Task<IRandomAccessStreamReference> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            await RandomAccessStream.CopyAndCloseAsync(memoryStream.AsInputStream(), outputStream);
            var result = RandomAccessStreamReference.CreateFromStream(randomAccessStream);
            return result;
        }
      
        private void lstHandWrittenNotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedImage = (Windows.UI.Xaml.Media.ImageSource)lstHandWrittenNotes.SelectedItem;
            displayInk.Source = selectedImage;
        }
       
        private async void LoadCanvasNote(List<string> base64stringList)
        {

            if (base64stringList.Any())
            {
                lstHandWrittenNotes.Items.Clear();
                foreach (var item in base64stringList)
                {
                    var bitmap = new BitmapImage();
                    var handwrittenimage = System.Convert.FromBase64String(item);

                    MemoryStream ms = new MemoryStream(handwrittenimage, 0, handwrittenimage.Length);
                    var randStream = await ConvertToRandomAccessStream(ms);


                    using (var stream = await randStream.OpenReadAsync())
                    {
                        await bitmap.SetSourceAsync(stream);
                    }

                    lstHandWrittenNotes.Items.Add(bitmap);


                }

            }

        }
       

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            dynamicNotes = string.Empty;
            string str = string.Empty;
            txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, string.IsNullOrEmpty(str) ? string.Empty : str);
            displayInk.Source = null;
        }
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            RecognizeText();
        }
        private async void RecognizeText()
        {

            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            InkRecognizerContainer inkRecognizerContainer = new InkRecognizerContainer();

            if (currentStrokes.Count > 0)
            {
                var recognitionResults = await inkRecognizerContainer.RecognizeAsync(inkCanvas.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

                if (recognitionResults.Count > 0)
                {
                    // Display recognition result
                    string str = "";
                    foreach (var r in recognitionResults)
                    {
                        str = str + " " + r.GetTextCandidates()[0];
                    }
                    txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, string.IsNullOrEmpty(str) ? string.Empty : str);

                }

                else
                {

                    txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, "No text recognized.");
                }
                string txt;
                txtrecognitionResult.Document.GetText(Windows.UI.Text.TextGetOptions.UseCrlf, out txt);
                dynamicNotes = dynamicNotes + " " + txt;

            }
            else
            {
                txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, "Must first write something.");
            }
        }

        private void loadInk_Click(object sender, RoutedEventArgs e)
        {
            #region Load image from Sqlitedb
            var base64string = vm.GetAllHandWritingNote();
            LoadCanvasNote(base64string);
            string str = vm.GetRecognizedNotes();
            txtrecognitionResult.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, string.IsNullOrEmpty(str) ? string.Empty : str);
            #endregion
        }
    }
}
