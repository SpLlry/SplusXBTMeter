using SplusXBTMeter.ViewModels.Base;

public class UpdateViewModel : ViewModelBase
{
    private string _updateText;
    private string _updateUrl;

    public string UpdateText
    {
        get => _updateText;
        set => SetProperty(ref _updateText, value);
    }

    public string UpdateUrl
    {
        get => UpdateUrl;
        set => SetProperty(ref _updateUrl, value);
    }


}