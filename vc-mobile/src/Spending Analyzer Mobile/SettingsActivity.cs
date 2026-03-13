using Spending_Analyzer_Mobile.Models;
using Spending_Analyzer_Mobile.Services;

namespace Spending_Analyzer_Mobile;

[Activity(Label = "Settings", Exported = false)]
public class SettingsActivity : Activity
{
    private EditText? _editHostUrl;
    private EditText? _editPort;
    private EditText? _editAccountId;
    private EditText? _editAccountName;
    private EditText? _editApiKey;
    private Button? _btnSaveSettings;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_settings);

        InitializeViews();
        LoadSettings();
        SetupClickListeners();
    }

    private void InitializeViews()
    {
        _editHostUrl = FindViewById<EditText>(Resource.Id.editHostUrl);
        _editPort = FindViewById<EditText>(Resource.Id.editPort);
        _editAccountId = FindViewById<EditText>(Resource.Id.editAccountId);
        _editAccountName = FindViewById<EditText>(Resource.Id.editAccountName);
        _editApiKey = FindViewById<EditText>(Resource.Id.editApiKey);
        _btnSaveSettings = FindViewById<Button>(Resource.Id.btnSaveSettings);
    }

    private async void LoadSettings()
    {
        try
        {
            var settings = await DatabaseService.Instance.GetSettingsAsync();
            if (settings != null)
            {
                RunOnUiThread(() =>
                {
                    _editHostUrl!.Text = settings.HostUrl;
                    _editPort!.Text = settings.Port.ToString();
                    _editAccountId!.Text = settings.AccountId;
                    _editAccountName!.Text = settings.AccountName;
                    _editApiKey!.Text = settings.ApiKey;
                });
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error loading settings: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void SetupClickListeners()
    {
        _btnSaveSettings!.Click += async (s, e) =>
        {
            await SaveSettings();
        };
    }

    private async Task SaveSettings()
    {
        try
        {
            if (!int.TryParse(_editPort!.Text, out var port))
            {
                Toast.MakeText(this, "Invalid port number", ToastLength.Short)?.Show();
                return;
            }

            var settings = new AppSettings
            {
                HostUrl = _editHostUrl!.Text ?? string.Empty,
                Port = port,
                AccountId = _editAccountId!.Text ?? string.Empty,
                AccountName = _editAccountName!.Text ?? string.Empty,
                ApiKey = _editApiKey!.Text ?? string.Empty
            };

            await DatabaseService.Instance.SaveSettingsAsync(settings);
            Toast.MakeText(this, "Settings saved successfully", ToastLength.Short)?.Show();
            Finish();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error saving settings: {ex.Message}", ToastLength.Short)?.Show();
        }
    }
}

