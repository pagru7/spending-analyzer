using Android.Content;
using Android.Views;
using Android.Widget;
using Spending_Analyzer_Mobile.Adapters;
using Spending_Analyzer_Mobile.Models;
using Spending_Analyzer_Mobile.Services;

namespace Spending_Analyzer_Mobile;

[Activity(Label = "Transactions")]
public class TransactionsActivity : Activity
{
    private ListView? _listTransactions;
    private ProgressBar? _progressBar;
    private TextView? _textEmpty;
    private LinearLayout? _filterPanel;
    private Button? _btnFilter;
    private Button? _btnStartDate;
    private Button? _btnEndDate;
    private EditText? _editFilterRecipient;
    private EditText? _editMinAmount;
    private EditText? _editMaxAmount;
    private Button? _btnApplyFilter;
    private Button? _btnClearFilter;

    private TransactionListAdapter? _adapter;
    private List<Transaction> _transactions = [];
    private bool _isLoading;
    private bool _hasMoreData = true;
    private int _pageSize = 20;
    private int _currentPage;

    // Filter values
    private DateTime? _filterStartDate;
    private DateTime? _filterEndDate;
    private string? _filterRecipient;
    private decimal? _filterMinAmount;
    private decimal? _filterMaxAmount;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_transactions);

        InitializeViews();
        SetupListView();
        SetupClickListeners();
        LoadTransactions();
    }

    private void InitializeViews()
    {
        _listTransactions = FindViewById<ListView>(Resource.Id.listTransactions);
        _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
        _textEmpty = FindViewById<TextView>(Resource.Id.textEmpty);
        _filterPanel = FindViewById<LinearLayout>(Resource.Id.filterPanel);
        _btnFilter = FindViewById<Button>(Resource.Id.btnFilter);
        _btnStartDate = FindViewById<Button>(Resource.Id.btnStartDate);
        _btnEndDate = FindViewById<Button>(Resource.Id.btnEndDate);
        _editFilterRecipient = FindViewById<EditText>(Resource.Id.editFilterRecipient);
        _editMinAmount = FindViewById<EditText>(Resource.Id.editMinAmount);
        _editMaxAmount = FindViewById<EditText>(Resource.Id.editMaxAmount);
        _btnApplyFilter = FindViewById<Button>(Resource.Id.btnApplyFilter);
        _btnClearFilter = FindViewById<Button>(Resource.Id.btnClearFilter);
    }

    private void SetupListView()
    {
        _adapter = new TransactionListAdapter(this, _transactions);
        _listTransactions!.Adapter = _adapter;

        _listTransactions.ItemClick += (s, e) =>
        {
            if (e.Position >= 0 && e.Position < _transactions.Count)
            {
                var transaction = _transactions[e.Position];
                var intent = new Intent(this, typeof(AddTransactionActivity));
                intent.PutExtra("TransactionId", transaction.Id);
                StartActivity(intent);
            }
        };

        _listTransactions.ScrollStateChanged += (s, e) =>
        {
            if (e.ScrollState == ScrollState.Idle)
            {
                var lastVisiblePosition = _listTransactions.LastVisiblePosition;
                if (lastVisiblePosition >= _transactions.Count - 5 && !_isLoading && _hasMoreData)
                {
                    LoadMoreTransactions();
                }
            }
        };
    }

    private void SetupClickListeners()
    {
        _btnFilter!.Click += (s, e) =>
        {
            _filterPanel!.Visibility = _filterPanel.Visibility == ViewStates.Visible
                ? ViewStates.Gone
                : ViewStates.Visible;
        };

        _btnStartDate!.Click += (s, e) =>
        {
            var date = _filterStartDate ?? DateTime.Now;
            var datePicker = new DatePickerDialog(this,
                (sender, args) =>
                {
                    _filterStartDate = new DateTime(args.Year, args.Month + 1, args.DayOfMonth);
                    _btnStartDate.Text = _filterStartDate.Value.ToString("yyyy-MM-dd");
                },
                date.Year, date.Month - 1, date.Day);
            datePicker.Show();
        };

        _btnEndDate!.Click += (s, e) =>
        {
            var date = _filterEndDate ?? DateTime.Now;
            var datePicker = new DatePickerDialog(this,
                (sender, args) =>
                {
                    _filterEndDate = new DateTime(args.Year, args.Month + 1, args.DayOfMonth, 23, 59, 59);
                    _btnEndDate.Text = _filterEndDate.Value.ToString("yyyy-MM-dd");
                },
                date.Year, date.Month - 1, date.Day);
            datePicker.Show();
        };

        _btnApplyFilter!.Click += (s, e) =>
        {
            ApplyFilters();
        };

        _btnClearFilter!.Click += (s, e) =>
        {
            ClearFilters();
        };
    }

    private void ApplyFilters()
    {
        _filterRecipient = string.IsNullOrWhiteSpace(_editFilterRecipient!.Text)
            ? null
            : _editFilterRecipient.Text;

        _filterMinAmount = decimal.TryParse(_editMinAmount!.Text, out var minAmt)
            ? minAmt
            : null;

        _filterMaxAmount = decimal.TryParse(_editMaxAmount!.Text, out var maxAmt)
            ? maxAmt
            : null;

        _transactions.Clear();
        _currentPage = 0;
        _hasMoreData = true;
        _adapter?.NotifyDataSetChanged();
        LoadTransactions();
    }

    private void ClearFilters()
    {
        _filterStartDate = null;
        _filterEndDate = null;
        _filterRecipient = null;
        _filterMinAmount = null;
        _filterMaxAmount = null;

        _btnStartDate!.Text = "Start Date";
        _btnEndDate!.Text = "End Date";
        _editFilterRecipient!.Text = string.Empty;
        _editMinAmount!.Text = string.Empty;
        _editMaxAmount!.Text = string.Empty;

        _transactions.Clear();
        _currentPage = 0;
        _hasMoreData = true;
        _adapter?.NotifyDataSetChanged();
        LoadTransactions();
    }

    private async void LoadTransactions()
    {
        if (_isLoading) return;
        _isLoading = true;

        RunOnUiThread(() =>
        {
            _progressBar!.Visibility = ViewStates.Visible;
            _textEmpty!.Visibility = ViewStates.Gone;
        });

        try
        {
            var skip = _currentPage * _pageSize;
            List<Transaction> newTransactions;

            if (HasActiveFilters())
            {
                newTransactions = await DatabaseService.Instance.GetFilteredTransactionsAsync(
                    _filterStartDate, _filterEndDate, _filterRecipient,
                    _filterMinAmount, _filterMaxAmount, skip, _pageSize);
            }
            else
            {
                newTransactions = await DatabaseService.Instance.GetTransactionsAsync(skip, _pageSize);
            }

            if (newTransactions.Count < _pageSize)
            {
                _hasMoreData = false;
            }

            _transactions.AddRange(newTransactions);
            _currentPage++;

            RunOnUiThread(() =>
            {
                _adapter?.NotifyDataSetChanged();
                _progressBar!.Visibility = ViewStates.Gone;
                _textEmpty!.Visibility = _transactions.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
            });
        }
        catch (Exception ex)
        {
            RunOnUiThread(() =>
            {
                _progressBar!.Visibility = ViewStates.Gone;
                Toast.MakeText(this, $"Error loading transactions: {ex.Message}", ToastLength.Short)?.Show();
            });
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void LoadMoreTransactions()
    {
        LoadTransactions();
    }

    private bool HasActiveFilters()
    {
        return _filterStartDate.HasValue || _filterEndDate.HasValue ||
               !string.IsNullOrWhiteSpace(_filterRecipient) ||
               _filterMinAmount.HasValue || _filterMaxAmount.HasValue;
    }

    protected override void OnResume()
    {
        base.OnResume();
        _transactions.Clear();
        _currentPage = 0;
        _hasMoreData = true;
        _adapter?.NotifyDataSetChanged();
        LoadTransactions();
    }
}
