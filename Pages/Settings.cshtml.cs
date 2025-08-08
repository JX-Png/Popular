using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PopularBookstore.Services;
using WebApplication1.Data;
using WebApplication1.Models;

namespace PopularBookstore.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CardEncryptionService _cardEncryptionService;

        public SettingsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CardEncryptionService cardEncryptionService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _cardEncryptionService = cardEncryptionService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public CardInputModel CardInput { get; set; } = new CardInputModel();

        public List<PaymentCard> UserCards { get; set; } = new List<PaymentCard>();

        [TempData]
        public string StatusMessage { get; set; }

        public int? EditCardId { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Delivery Address")]
            public string Address { get; set; }
        }

        public class CardInputModel
        {
            [Required]
            [Display(Name = "Card Name")]
            public string CardName { get; set; }

            [Required]
            // Removed [CreditCard] attribute to allow any 16 digits
            [Display(Name = "Card Number")]
            [RegularExpression(@"^\d{16}$", ErrorMessage = "Please enter 16 digits")]
            public string CardNumber { get; set; }

            [Required]
            [Display(Name = "Cardholder Name")]
            public string CardholderName { get; set; }

            [Required]
            [Display(Name = "Expiry Month")]
            [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Please enter a valid month (01-12)")]
            public string ExpiryMonth { get; set; }

            [Required]
            [Display(Name = "Expiry Year")]
            [RegularExpression(@"^\d{2}$", ErrorMessage = "Please enter a valid 2-digit year")]
            public string ExpiryYear { get; set; }

            public bool IsDefault { get; set; }
        }

        private async Task LoadCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Input = new InputModel
            {
                Name = user.Name,
                Email = user.Email,
                Address = user.Address
            };
        }

        private async Task LoadUserCards(string userId)
        {
            UserCards = await _context.PaymentCards
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IsDefault)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(int? editId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login", new { returnUrl = "/Settings" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadCurrentUserAsync();
            await LoadUserCards(user.Id);

            if (editId.HasValue)
            {
                var cardToEdit = UserCards.FirstOrDefault(c => c.Id == editId.Value);
                if (cardToEdit != null)
                {
                    EditCardId = cardToEdit.Id;
                    CardInput = new CardInputModel
                    {
                        CardName = cardToEdit.CardName,
                        CardholderName = cardToEdit.CardholderName,
                        ExpiryMonth = cardToEdit.ExpiryMonth,
                        ExpiryYear = cardToEdit.ExpiryYear,
                        IsDefault = cardToEdit.IsDefault
                    };
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCurrentUserAsync();
                await LoadUserCards(user.Id);
                return Page();
            }

            // Update Name and Address
            user.Name = Input.Name;
            user.Address = Input.Address;

            // Update Email and UserName if changed
            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    StatusMessage = "Error: Unexpected error when trying to set email.";
                    return RedirectToPage();
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Email);
                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "Error: Unexpected error when trying to set user name.";
                    return RedirectToPage();
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Error: Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddCardAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                await LoadCurrentUserAsync();
                await LoadUserCards(user.Id);
                return Page();
            }

            // Encrypt card number
            var encryptedCardNumber = _cardEncryptionService.EncryptCardNumber(CardInput.CardNumber);
            var lastFourDigits = _cardEncryptionService.GetLastFourDigits(CardInput.CardNumber);

            // Check if this should be the default card
            bool setAsDefault = CardInput.IsDefault || !UserCards.Any();

            // If setting as default, unset other default cards
            if (setAsDefault)
            {
                var defaultCards = await _context.PaymentCards
                    .Where(c => c.UserId == user.Id && c.IsDefault)
                    .ToListAsync();

                foreach (var card in defaultCards)
                {
                    card.IsDefault = false;
                }
            }

            // Create new card
            var newCard = new PaymentCard
            {
                UserId = user.Id,
                CardName = CardInput.CardName,
                CardNumberHash = encryptedCardNumber,
                CardholderName = CardInput.CardholderName,
                ExpiryMonth = CardInput.ExpiryMonth,
                ExpiryYear = CardInput.ExpiryYear,
                CardNumberLastFour = lastFourDigits,
                IsDefault = setAsDefault
            };

            _context.PaymentCards.Add(newCard);
            await _context.SaveChangesAsync();

            StatusMessage = "Your card has been added.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateCardAsync(int cardId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var user = await _userManager.GetUserAsync(User);
            var card = await _context.PaymentCards
                .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == user.Id);

            if (card == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadCurrentUserAsync();
                await LoadUserCards(user.Id);
                EditCardId = cardId;
                return Page();
            }

            // Update card details
            card.CardName = CardInput.CardName;
            card.CardholderName = CardInput.CardholderName;
            card.ExpiryMonth = CardInput.ExpiryMonth;
            card.ExpiryYear = CardInput.ExpiryYear;

            // Only update card number if a new one was provided
            if (!string.IsNullOrEmpty(CardInput.CardNumber))
            {
                card.CardNumberHash = _cardEncryptionService.EncryptCardNumber(CardInput.CardNumber);
                card.CardNumberLastFour = _cardEncryptionService.GetLastFourDigits(CardInput.CardNumber);
            }

            // Handle default card status
            if (CardInput.IsDefault && !card.IsDefault)
            {
                var defaultCards = await _context.PaymentCards
                    .Where(c => c.UserId == user.Id && c.IsDefault)
                    .ToListAsync();

                foreach (var defaultCard in defaultCards)
                {
                    defaultCard.IsDefault = false;
                }

                card.IsDefault = true;
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Your card has been updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCardAsync(int cardId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var user = await _userManager.GetUserAsync(User);
            var card = await _context.PaymentCards
                .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == user.Id);

            if (card == null)
            {
                return NotFound();
            }

            _context.PaymentCards.Remove(card);
            await _context.SaveChangesAsync();

            StatusMessage = "Your card has been deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetDefaultCardAsync(int cardId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var user = await _userManager.GetUserAsync(User);

            // Reset all cards to non-default
            var userCards = await _context.PaymentCards
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            foreach (var card in userCards)
            {
                card.IsDefault = (card.Id == cardId);
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Default payment method updated.";
            return RedirectToPage();
        }
    }
}