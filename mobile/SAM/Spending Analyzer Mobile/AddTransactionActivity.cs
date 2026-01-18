using Spending_Analyzer_Mobile.Models;
using Spending_Analyzer_Mobile.Services;

namespace Spending_Analyzer_Mobile;

[Activity(Label = "Add Expense")]
public class AddTransactionActivity : Activity
{
    private TextView? _textTitle;
    private EditText? _editAmount;
    private EditText? _editRecipient;
    private EditText? _editDescription;
    private Button? _btnSelectDate;
    private Button? _btnSelectTime;
    private Button? _btnSave;
    private Button? _btnDelete;

    private DateTime _selectedDateTime = DateTime.Now;
    private int _transactionId;
    private Transaction? _existingTransaction;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_add_transaction);

        _transactionId = Intent?.GetIntExtra("TransactionId", 0) ?? 0;

        InitializeViews();
        SetupClickListeners();

        if (_transactionId > 0)
        {
            LoadTransaction();
        }
        else
        {
            UpdateDateTimeButtons();
        }
    }

    private void InitializeViews()
    {
        _textTitle = FindViewById<TextView>(Resource.Id.textTitle);
        _editAmount = FindViewById<EditText>(Resource.Id.editAmount);
        _editRecipient = FindViewById<EditText>(Resource.Id.editRecipient);
        _editDescription = FindViewById<EditText>(Resource.Id.editDescription);
        _btnSelectDate = FindViewById<Button>(Resource.Id.btnSelectDate);
        _btnSelectTime = FindViewById<Button>(Resource.Id.btnSelectTime);
        _btnSave = FindViewById<Button>(Resource.Id.btnSave);
        _btnDelete = FindViewById<Button>(Resource.Id.btnDelete);
    }

    private async void LoadTransaction()
    {
        try
        {
            _existingTransaction = await DatabaseService.Instance.GetTransactionAsync(_transactionId);
            if (_existingTransaction != null)
            {
                _selectedDateTime = _existingTransaction.TransactionDate;

                RunOnUiThread(() =>
                {
                    _textTitle!.Text = "Edit Expense";
                    _editAmount!.Text = _existingTransaction.Amount.ToString("F2");
                    _editRecipient!.Text = _existingTransaction.Recipient;
                    _editDescription!.Text = _existingTransaction.Description;
                    _btnDelete!.Visibility = Android.Views.ViewStates.Visible;
                    UpdateDateTimeButtons();
                });
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error loading transaction: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void SetupClickListeners()
    {
        _btnSelectDate!.Click += (s, e) =>
        {
            var datePicker = new DatePickerDialog(this,
                (sender, args) =>
                {
                    _selectedDateTime = new DateTime(args.Year, args.Month + 1, args.DayOfMonth,
                        _selectedDateTime.Hour, _selectedDateTime.Minute, _selectedDateTime.Second);
                    UpdateDateTimeButtons();
                },
                _selectedDateTime.Year, _selectedDateTime.Month - 1, _selectedDateTime.Day);
            datePicker.Show();
        };

        _btnSelectTime!.Click += (s, e) =>
        {
            var timePicker = new TimePickerDialog(this,
                (sender, args) =>
                {
                    _selectedDateTime = new DateTime(_selectedDateTime.Year, _selectedDateTime.Month, _selectedDateTime.Day,
                        args.HourOfDay, args.Minute, 0);
                    UpdateDateTimeButtons();
                },
                _selectedDateTime.Hour, _selectedDateTime.Minute, true);
            timePicker.Show();
        };

        _btnSave!.Click += async (s, e) =>
        {
            await SaveTransaction();
        };

        _btnDelete!.Click += (s, e) =>
        {
            ConfirmDelete();
        };
    }

    private void UpdateDateTimeButtons()
    {
        _btnSelectDate!.Text = _selectedDateTime.ToString("yyyy-MM-dd");
        _btnSelectTime!.Text = _selectedDateTime.ToString("HH:mm");
    }

    private async Task SaveTransaction()
    {
        try
        {
            if (!decimal.TryParse(_editAmount!.Text, out var amount) || amount <= 0)
            {
                Toast.MakeText(this, "Please enter a valid amount", ToastLength.Short)?.Show();
                return;
            }

            if (string.IsNullOrWhiteSpace(_editRecipient!.Text))
            {
                Toast.MakeText(this, "Please enter a recipient", ToastLength.Short)?.Show();
                return;
            }

            var transaction = _existingTransaction ?? new Transaction();
            transaction.Amount = amount;
            transaction.Recipient = _editRecipient.Text ?? string.Empty;
            transaction.Description = _editDescription!.Text ?? string.Empty;
            transaction.TransactionDate = _selectedDateTime;

            if (_existingTransaction != null)
            {
                transaction.IsSynchronized = false;
            }

            await DatabaseService.Instance.SaveTransactionAsync(transaction);
            Toast.MakeText(this, "Transaction saved", ToastLength.Short)?.Show();
            Finish();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error saving transaction: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void ConfirmDelete()
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Delete Transaction");
        builder.SetMessage("Are you sure you want to delete this transaction?");

        builder.SetPositiveButton("Delete", async (s, e) =>
        {
            await DeleteTransaction();
        });

        builder.SetNegativeButton("Cancel", (s, e) => { });

        builder.Show();
    }

    private async Task DeleteTransaction()
    {
        try
        {
            if (_existingTransaction != null)
            {
                await DatabaseService.Instance.DeleteTransactionAsync(_existingTransaction);
                Toast.MakeText(this, "Transaction deleted", ToastLength.Short)?.Show();
                Finish();
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error deleting transaction: {ex.Message}", ToastLength.Short)?.Show();
        }
    }
}
