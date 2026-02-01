using Android.Views;
using Android.Widget;
using Spending_Analyzer_Mobile.Models;

namespace Spending_Analyzer_Mobile.Adapters;

public class TransactionListAdapter : BaseAdapter<Transaction>
{
    private readonly Activity _context;
    private readonly List<Transaction> _transactions;

    public TransactionListAdapter(Activity context, List<Transaction> transactions)
    {
        _context = context;
        _transactions = transactions;
    }

    public override Transaction this[int position] => _transactions[position];

    public override int Count => _transactions.Count;

    public override long GetItemId(int position) => _transactions[position].Id;

    public override View? GetView(int position, View? convertView, ViewGroup? parent)
    {
        var view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.item_transaction, parent, false);

        if (view == null) return null;

        var transaction = _transactions[position];

        var textRecipient = view.FindViewById<TextView>(Resource.Id.textRecipient);
        var textDescription = view.FindViewById<TextView>(Resource.Id.textDescription);
        var textType = view.FindViewById<TextView>(Resource.Id.textType);
        var textDate = view.FindViewById<TextView>(Resource.Id.textDate);
        var textAmount = view.FindViewById<TextView>(Resource.Id.textAmount);
        var textBalance = view.FindViewById<TextView>(Resource.Id.textBalance);
        var iconSync = view.FindViewById<ImageView>(Resource.Id.iconSync);

        if (textRecipient != null)
            textRecipient.Text = transaction.Recipient;

        if (textDescription != null)
            textDescription.Text = string.IsNullOrWhiteSpace(transaction.Description)
                ? "No description"
                : transaction.Description;

        if (textType != null)
            textType.Text = transaction.TransactionType;

        if (textDate != null)
            textDate.Text = transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm");

        if (textAmount != null)
        {
            var signedAmount = transaction.TransactionType switch
            {
                TransactionTypes.Income => transaction.Amount,
                TransactionTypes.Transfer => 0m,
                _ => -transaction.Amount
            };
            textAmount.Text = $"{signedAmount:+$0.00;-$0.00;$0.00}";
            textAmount.SetTextColor(transaction.TransactionType == TransactionTypes.Income
                ? Android.Graphics.Color.ParseColor("#4CAF50")
                : transaction.TransactionType == TransactionTypes.Transfer
                    ? Android.Graphics.Color.ParseColor("#607D8B")
                    : Android.Graphics.Color.ParseColor("#F44336"));
        }

        if (textBalance != null)
            textBalance.Text = $"Balance: ${transaction.Balance:N2}";

        if (iconSync != null)
            iconSync.Visibility = transaction.IsSynchronized
                ? ViewStates.Visible
                : ViewStates.Gone;

        return view;
    }
}
