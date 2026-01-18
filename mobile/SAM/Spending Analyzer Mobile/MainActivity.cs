using Android.Content;
using Spending_Analyzer_Mobile.Services;

namespace Spending_Analyzer_Mobile;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView? _textBalance;
    private TextView? _textTransactionCount;
    private Button? _btnAddExpense;
    private Button? _btnViewTransactions;
    private Button? _btnExportData;
    private Button? _btnSettings;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);

        InitializeViews();
        SetupClickListeners();
    }

    protected override void OnResume()
    {
        base.OnResume();
        LoadDashboardData();
    }

    private void InitializeViews()
    {
        _textBalance = FindViewById<TextView>(Resource.Id.textBalance);
        _textTransactionCount = FindViewById<TextView>(Resource.Id.textTransactionCount);
        _btnAddExpense = FindViewById<Button>(Resource.Id.btnAddExpense);
        _btnViewTransactions = FindViewById<Button>(Resource.Id.btnViewTransactions);
        _btnExportData = FindViewById<Button>(Resource.Id.btnExportData);
        _btnSettings = FindViewById<Button>(Resource.Id.btnSettings);
    }

    private void SetupClickListeners()
    {
        _btnAddExpense!.Click += (s, e) =>
        {
            var intent = new Intent(this, typeof(AddTransactionActivity));
            StartActivity(intent);
        };

        _btnViewTransactions!.Click += (s, e) =>
        {
            var intent = new Intent(this, typeof(TransactionsActivity));
            StartActivity(intent);
        };

        _btnExportData!.Click += (s, e) =>
        {
            ShowExportDialog();
        };

        _btnSettings!.Click += (s, e) =>
        {
            var intent = new Intent(this, typeof(SettingsActivity));
            StartActivity(intent);
        };
    }

    private async void LoadDashboardData()
    {
        try
        {
            var db = DatabaseService.Instance;
            var settings = await db.GetSettingsAsync();
            var totalSpending = await db.GetTotalSpendingAsync();
            var transactionCount = await db.GetTransactionCountAsync();

            var balance = (settings?.InitialBalance ?? 0) - totalSpending;

            RunOnUiThread(() =>
            {
                _textBalance!.Text = $"${balance:N2}";
                _textBalance.SetTextColor(balance >= 0
                    ? Android.Graphics.Color.ParseColor("#4CAF50")
                    : Android.Graphics.Color.ParseColor("#F44336"));
                _textTransactionCount!.Text = $"{transactionCount} transaction(s)";
            });
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error loading data: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void ShowExportDialog()
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Export Data");
        builder.SetMessage("Choose export option:");

        builder.SetPositiveButton("Sync All", async (s, e) =>
        {
            await ExportData(syncAll: true);
        });

        builder.SetNegativeButton("Sync New Only", async (s, e) =>
        {
            await ExportData(syncAll: false);
        });

        builder.SetNeutralButton("Cancel", (s, e) => { });

        builder.Show();
    }

    private async Task ExportData(bool syncAll)
    {
        var progressDialog = new ProgressDialog(this);
        progressDialog.SetMessage("Syncing data...");
        progressDialog.SetCancelable(false);
        progressDialog.Show();

        try
        {
            var apiService = new ApiService();
            var result = syncAll
                ? await apiService.SyncAllTransactionsAsync()
                : await apiService.SyncNewTransactionsAsync();

            progressDialog.Dismiss();

            var message = result.Success
                ? result.Message
                : $"Sync failed: {result.Message}";

            Toast.MakeText(this, message, ToastLength.Long)?.Show();
            LoadDashboardData();
        }
        catch (Exception ex)
        {
            progressDialog.Dismiss();
            Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Long)?.Show();
        }
    }
}
