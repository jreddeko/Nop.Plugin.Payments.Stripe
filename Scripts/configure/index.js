
$(document).ready(function () {
    var stripe = Stripe(''); // enter public key here
    $("#wizard").steps();
    $("#form").steps({
        bodyTag: "fieldset",
        onStepChanging: function (event, currentIndex, newIndex) {
            // Always allow going backward even if the current step contains invalid fields!
            if (currentIndex > newIndex) {
                return true;
            }
            var form = $(this);

            // Clean up if user went backward before
            if (currentIndex < newIndex) {
                // To remove error styles
                $(".body:eq(" + newIndex + ") label.error", form).remove();
                $(".body:eq(" + newIndex + ") .error", form).removeClass("error");
            }

            // Disable validation on fields that are disabled or hidden.
            form.validate().settings.ignore = ":disabled,:hidden";

            // Start validation; Prevent going forward if false
            return form.valid();
        },
        onStepChanged: function (event, currentIndex, priorIndex) {

        },
        onFinishing: function (event, currentIndex) {
            var form = $(this);

            $('#account-wizard').children('.ibox-content').toggleClass('sk-loading');
            // Disable validation on fields that are disabled.
            // At this point it's recommended to do an overall check (mean ignoring only disabled fields)
            form.validate().settings.ignore = ":disabled";

            // Start validation; Prevent form submission if false
            return form.valid();

        },
        onFinished: function (event, currentIndex) {
            var form = $(this);

            var ba = stripe.createToken('bank_account', {
                country: $('#banking-country').val(),
                currency: 'cad',
                routing_number: $('#banking-routing-number').val(),
                account_number: $('#banking-account-number').val(),
                account_holder_name: $('#BankAccountAccountHolderName').val(),
                account_holder_type: 'company',
            });
            var pii = function myAsyncFunction(url) {
                return new Promise((resolve, reject) => {
                    if (legalEntityPersonalIdNumberProvided) {
                        resolve();
                    }
                    else {
                        const stripeResponse = stripe.createToken('pii', { personal_id_number: $('#personal-id-number').val() });
                        if (stripeResponse.error != null) {
                            reject(stripeResponse.error.message)
                        }
                        else {
                            resolve(stripeResponse.token)
                        }
                    }
                });
            }           

            Promise.all([ba, pii]).then(values => {
                if (values[0].error != null) {
                    assert.ok(false, values[0].error.message);
                    done();
                }
                else if (values[1].error != null) {
                    assert.ok(false, values[1].error.message);
                    done();
                }
                else {
                    $('#BankAccountToken').val(values[0].token.id)
                    $('#PiiToken').val(values[1].id);
                    $.post('/PaymentStripe/Configure/', form.serialize())
                    .done(function (result) {
                        serialize = function (obj, prefix) {
                            var str = [], p;
                            for (p in obj) {
                                if (obj.hasOwnProperty(p)) {
                                    var k = prefix ? prefix + "[" + p + "]" : p, v = obj[p];
                                    str.push((v !== null && typeof v === "object") ?
                                      serialize(v, k) :
                                      encodeURIComponent(k) + "=" + encodeURIComponent(v));
                                }
                            }
                            return str.join("&");
                        }
                        $('#account-wizard').children('.ibox-content').toggleClass('sk-loading');
                        $('#AccountId').val(result.accountId)
                        $('#Issues').val(result.issues)
                        form.submit();
                    })
                    .fail(function (jqXHR, textStatus, errorThrown) {
                        $('#account-wizard').children('.ibox-content').toggleClass('sk-loading');
                        alert(errorThrown)
                    });
                }
            });
        }
    }).validate({
        errorPlacement: function (error, element) {
            element.before(error);
        },
    });

});
