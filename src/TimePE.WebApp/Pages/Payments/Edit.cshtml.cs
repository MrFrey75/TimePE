using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;
using DevExpress.Xpo;

namespace TimePE.WebApp.Pages.Payments;

public class EditModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public EditModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public DateTime Date { get; set; }

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound();

        Id = payment.Oid;
        Date = payment.Date;
        Amount = payment.Amount;
        Notes = payment.Notes;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var payment = await _paymentService.GetPaymentByIdAsync(Id);
        if (payment == null)
            return NotFound();

        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var editPayment = uow.GetObjectByKey<Payment>(Id);
        if (editPayment != null)
        {
            editPayment.Date = Date;
            editPayment.Amount = Amount;
            editPayment.Notes = Notes;
        }

        await _paymentService.UpdatePaymentAsync(editPayment!);
        TempData["SuccessMessage"] = "Payment updated successfully!";
        return RedirectToPage("Index");
    }
}
