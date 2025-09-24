using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace UclePdf.Views;

public partial class PreviewPdfWindow : Window
{
    private byte[] _pdfBytes = Array.Empty<byte>();

    public PreviewPdfWindow(byte[] pdfBytes)
    {
        InitializeComponent();
        _pdfBytes = pdfBytes;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await Browser.EnsureCoreWebView2Async();
        var base64 = Convert.ToBase64String(_pdfBytes);
        // Use <embed> to show PDF if Edge supports internal viewer
        var html = $"<html><body style='margin:0;padding:0;background:#444;'><embed src='data:application/pdf;base64,{base64}' type='application/pdf' width='100%' height='100%'/></body></html>";
        var tempFile = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid():N}.html");
        File.WriteAllText(tempFile, html);
        Browser.Source = new Uri(tempFile);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "InformesUCLE");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"Informe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            File.WriteAllBytes(file, _pdfBytes);
            MessageBox.Show($"Guardado en: {file}", "PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error guardando: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
