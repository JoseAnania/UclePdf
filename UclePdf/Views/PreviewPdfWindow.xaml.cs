using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace UclePdf.Views;

public partial class PreviewPdfWindow : Window
{
    private readonly byte[] _pdfBytes;

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
        var html = $"<html><body style='margin:0;padding:0;background:#444;'><embed src='data:application/pdf;base64,{base64}' type='application/pdf' width='100%' height='100%'/></body></html>";
        var tempFile = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid():N}.html");
        File.WriteAllText(tempFile, html);
        Browser.Source = new Uri(tempFile);
    }
}
