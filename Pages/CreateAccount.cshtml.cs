using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PopularBookstore.Services; // Your email service namespace
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class CreateAccountModel : PageModel
{
    private readonly IEmailSender _emailSender;

    public CreateAccountModel(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public bool ShowConfirmationMessage { get; set; } = false;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }
    }

    public void OnGet()
    {
        ShowConfirmationMessage = false;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // In a real application, you would:
        // 1. Check if the user already exists.
        // 2. Hash the password securely.
        // 3. Save the user to a database (e.g., using ASP.NET Core Identity).

        // For now, we'll simulate user creation and send the email.
        var subject = "Welcome to Popular Bookstore!";
        var message = $"Hello {Input.Name},<br><br>Thank you for creating an account at Popular Bookstore. We're excited to have you!<br><br>You can now log in using your email: {Input.Email}.<br><br>Best regards,<br>The Popular Bookstore Team";
        
        await _emailSender.SendEmailAsync(Input.Email, subject, message);

        ShowConfirmationMessage = true;
        // Optionally clear the input model if you don't want to redisplay the data
        // Input = new InputModel(); 
        
        return Page();
    }
}