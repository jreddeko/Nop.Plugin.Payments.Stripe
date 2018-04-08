'use strict';

// must declare stripe before using index.js 
// eg. var stripe = Stripe(model.StripePublicApiKey);
function postStripeToken(stripeApplication, sourceId, accountId) {
    var form = $('form')[1];
    var error = form.querySelector('.error');
    var errorMessage = error.querySelector('.message');
    return $.post("/paymentstripe/paymentinfo", {
        SourceId: sourceId,
        AccountId: accountId,
    })
}

function registerElements(elements, layoutName) {
    var formClass = '.' + layoutName;
    var stripeApplication = document.querySelector(formClass);
    var form = $('form')[1];
    var resetButton = stripeApplication.querySelector('a.reset');
    var error = form.querySelector('.error');
    var errorMessage = error.querySelector('.message');

    // card has been charged from 3d secure and result returned
    if (model.StripeCharge != null)
        processResult(stripeApplication, model.StripeCharge);

    // card has not yet been charged or has tried 3d secure auth and failed
    if (model.StripeSource != null)
        processResult(stripeApplication, model.StripeSource);


    // Listen for errors from each Element, and show error messages in the UI.
    elements.forEach(function (element) {
        element.on('change', function (event) {
            if (event.error) {
                error.classList.add('visible');
                errorMessage.innerText = event.error.message;
            } else {
                error.classList.remove('visible');
            }
        });
    });

    // Listen on the form's 'submit' handler...
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        // Show a loading screen...
        stripeApplication.classList.add('submitting');

        // Disable all inputs.
        disableInputs();


        // Gather additional customer data we may have collected in our form.
        var name = form.querySelector('#' + layoutName + '-name');
        var address1 = form.querySelector('#' + layoutName + '-address');
        var city = form.querySelector('#' + layoutName + '-city');
        var state = form.querySelector('#' + layoutName + '-state');
        var zip = form.querySelector('#' + layoutName + '-zip');
        var additionalData = {
            type: 'card',
            currency: 'cad',
            usage: 'single_use',
            amount: model.Amount,
            owner: {
                name: name ? name.value : undefined,
                address: {
                    line1: address1 ? address1.value : undefined,
                    postal_code: zip ? zip.value : undefined,
                    state: state ? state.value : undefined,
                    city: city ? city.value : undefined,
                    name: name ? name.value : undefined,
                },
            },
        };

        // Use Stripe.js to create a token. We only need to pass in one Element
        // from the Element group in order to create a token. We can also pass
        // in the additional customer data we collected in our form.
        createSource(elements[0], additionalData);
    });

    resetButton.addEventListener('click', function (e) {
        e.preventDefault();
        // Resetting the form (instead of setting the value to `''` for each input)
        // helps us clear webkit autofill styles.
        form.reset();

        // Clear each Element.
        elements.forEach(function (element) {
            element.clear();
        });

        // Reset error state as well.
        error.classList.remove('visible');

        // Resetting the form does not un-disable inputs, so we need to do it separately:
        enableInputs();
        stripeApplication.classList.remove('submitted');
    });

    function processResult(stripeApplication, result) {
        if (result.Status === 'succeeded') {
            stripeApplication.classList.remove('submitting');
            stripeApplication.querySelector('.success .message').innerText = result.Outcome.SellerMessage;
            stripeApplication.classList.add('submitted');
        }
        else {
            stripeApplication.classList.remove('submitting');
            error.classList.add('visible');
            errorMessage.innerText = result.message == null ? 'Payment failed' : result.message;
        }
    }

    function createSource(element, additionalData) {
        return stripe.createSource(element, additionalData).then(function (result) {
            if (result.error) {

                stripeApplication.classList.remove('submitting');
                // Otherwise, un-disable inputs.
                enableInputs();
            }
            else {
                postStripeToken(stripeApplication, result.source.id, $('#AccountId').val())
                    .done(function (result) {
                        if (result.Flow === 'redirect' && result.Status === 'pending') {
                            //$(stripeApplication).load(result.Redirect.Url);                        
                            window.location.href = result.Redirect.Url;
                        }
                        else {
                            processResult(stripeApplication, result);
                        }
                    })
                    .fail(function (jqXHR, textStatus, errorThrown) {
                        stripeApplication.classList.remove('submitting');
                        error.classList.add('visible');
                        errorMessage.innerText = errorThrown;
                    });
            }
        });
    }

    function enableInputs() {

        var form = $('form')[1];
        Array.prototype.forEach.call(
          form.querySelectorAll(
            "input[type='text'], input[type='email'], input[type='tel']"
          ),
          function (input) {
              input.removeAttribute('disabled');
          }
        );
    }

    function disableInputs() {
        var form = $('form')[1];
        Array.prototype.forEach.call(
          form.querySelectorAll(
            "input[type='text'], input[type='email'], input[type='tel']"
          ),
          function (input) {
              input.setAttribute('disabled', 'true');
          }
        );
    }
}
