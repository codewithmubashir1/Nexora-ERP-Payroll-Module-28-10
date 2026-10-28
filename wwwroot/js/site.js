// ============================================================
// Global Notyf instance
// ============================================================
window.notyf = new Notyf({
    duration: 3500,
    position: { x: 'right', y: 'top' },
    ripple: true,
    types: [
        { type: 'success', background: '#28a745', icon: { className: 'fa-solid fa-check', tagName: 'i', color: '#fff' } },
        { type: 'error', background: '#dc3545', icon: { className: 'fa-solid fa-triangle-exclamation', tagName: 'i', color: '#fff' } }
    ]
});

function initializeApplicationDropdowns(root) {
    if (!window.jQuery || !jQuery.fn.select2) return;

    var scope = jQuery(root || document);
    scope.find('select').addBack('select')
        .not('.select2-hidden-accessible, .dt-input')
        .filter(function () { return !jQuery(this).closest('.dt-container').length; })
        .each(function () {
            var select = jQuery(this);
            var modal = select.closest('.modal');
            select.select2({
                theme: 'bootstrap-5',
                width: '100%',
                placeholder: select.data('placeholder') || 'Select...',
                dropdownParent: modal.length ? modal : jQuery(document.body)
            });
        });
}

function initializeDataTableLengthDropdowns(root) {
    if (!window.jQuery || !jQuery.fn.select2) return;

    var scope = jQuery(root || document);
    scope.find('.dt-length select').addBack('.dt-length select')
        .not('.select2-hidden-accessible')
        .filter(function () { return jQuery(this).closest('.dt-container').length > 0; })
        .each(function () {
            var select = jQuery(this);
            select.select2({
                theme: 'bootstrap-5',
                width: '90px',
                minimumResultsForSearch: Infinity,
                dropdownParent: jQuery(document.body)
            });
        });
}

document.addEventListener('DOMContentLoaded', function () {

    // ------------------------------------------------------------
    // DataTables — fully-featured defaults applied to every table in
    // the app (export buttons, column visibility, responsive collapse,
    // paging/length/search/info). Individual pages only need to supply
    // `ajax` + `columns`; everything else is inherited from here and
    // can still be overridden per-table if needed. Must run before any
    // page's own .DataTable() call, so it lives at the very top here.
    // ------------------------------------------------------------
    if (window.jQuery && jQuery.fn.DataTable) {
        jQuery.extend(true, jQuery.fn.DataTable.defaults, {
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
            dom: "<'row align-items-center mb-2 dt-toolbar-row'<'col-md-7'B><'col-md-5 d-flex justify-content-md-end align-items-center gap-2'lf>>" +
                 "<'row'<'col-sm-12't>>" +
                 "<'row mt-2 align-items-center'<'col-sm-5'i><'col-sm-7'p>>",
            buttons: [
                { extend: 'copyHtml5', className: 'btn btn-outline-secondary btn-sm text-white', text: '<i class="fa-solid fa-copy me-1"></i>Copy', titleAttr: 'Copy visible rows to clipboard' },
                { extend: 'csvHtml5', className: 'btn btn-outline-success btn-sm text-white', text: '<i class="fa-solid fa-file-csv me-1"></i>CSV', titleAttr: 'Download as CSV' },
                { extend: 'excelHtml5', className: 'btn btn-outline-success btn-sm text-white', text: '<i class="fa-solid fa-file-excel me-1"></i>Excel', titleAttr: 'Download as Excel' },
                { extend: 'pdfHtml5', className: 'btn btn-outline-danger btn-sm text-white', text: '<i class="fa-solid fa-file-pdf me-1"></i>PDF', titleAttr: 'Download as PDF' },
                { extend: 'print', className: 'btn btn-outline-primary btn-sm text-white', text: '<i class="fa-solid fa-print me-1"></i>Print', titleAttr: 'Print visible rows' },
                { extend: 'colvis', className: 'btn btn-outline-secondary btn-sm text-white', text: '<i class="fa-solid fa-table-columns me-1"></i>Columns', titleAttr: 'Show/hide columns' }
            ],
            language: {
                search: '',
                searchPlaceholder: 'Search...',
                lengthMenu: '_MENU_ per page',
                info: 'Showing _START_ to _END_ of _TOTAL_ entries',
                infoEmpty: 'No records found',
                infoFiltered: '(filtered from _MAX_ total entries)',
                paginate: { previous: '<i class="fa-solid fa-chevron-left"></i>', next: '<i class="fa-solid fa-chevron-right"></i>' }
            }
        });

        initializeDataTableLengthDropdowns(document);
        if (window.MutationObserver) {
            var dataTableControlObserver = new MutationObserver(function (mutations) {
                mutations.forEach(function (mutation) {
                    mutation.addedNodes.forEach(function (node) {
                        if (node.nodeType === Node.ELEMENT_NODE && (node.matches('.dt-container') || node.querySelector('.dt-container'))) {
                            initializeDataTableLengthDropdowns(node);
                        }
                    });
                });
            });
            dataTableControlObserver.observe(document.body, { childList: true, subtree: true });
        }
    }

    // ------------------------------------------------------------
    // Low stock alert bell — fetched once per page load. Fails silently
    // if the endpoint isn't reachable (e.g. logged-out state).
    // ------------------------------------------------------------
    var alertsList = document.getElementById('alertsList');
    var alertsBadge = document.getElementById('alertsBadge');
    if (alertsList && alertsBadge) {
        fetch('/Inventory/LowStockAlerts')
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (data) {
                if (!data) return;

                if (data.count > 0) {
                    alertsBadge.textContent = data.count > 99 ? '99+' : data.count;
                    alertsBadge.classList.remove('d-none');
                } else {
                    alertsBadge.classList.add('d-none');
                }

                if (!data.alerts || data.alerts.length === 0) {
                    alertsList.innerHTML = '<p class="text-muted small text-center py-3 mb-0">No stock alerts. Everything looks good.</p>';
                    return;
                }

                alertsList.innerHTML = data.alerts.map(function (a) {
                    var badgeCls = a.status === 'Out' ? 'bg-danger' : 'bg-warning';
                    var label = a.status === 'Out' ? 'Out of stock' : 'Low stock';
                    return `
                        <a href="/Inventory/Products/Edit/${a.id}" class="d-flex justify-content-between align-items-center text-decoration-none p-2 rounded quick-link">
                            <span>
                                <span class="d-block small fw-semibold text-dark">${a.name}</span>
                                <span class="d-block text-muted" style="font-size:.75rem;">${a.sku} — ${label}</span>
                            </span>
                            <span class="badge ${badgeCls}">${a.currentStock}/${a.reorderLevel}</span>
                        </a>`;
                }).join('');
            })
            .catch(function () { /* silent — non-critical widget */ });
    }

    // ------------------------------------------------------------
    // Surface TempData messages (set by controllers) as Notyf toasts
    // ------------------------------------------------------------
    var toastEl = document.getElementById('serverToast');
    if (toastEl) {
        var successMsg = toastEl.getAttribute('data-success');
        var errorMsg = toastEl.getAttribute('data-error');
        if (successMsg) notyf.success(successMsg);
        if (errorMsg) notyf.error(errorMsg);
    }

    // ------------------------------------------------------------
    // Logout via SweetAlert2 confirmation
    // ------------------------------------------------------------
    var logoutLink = document.getElementById('logoutLink');
    if (logoutLink && window.Swal) {
        logoutLink.addEventListener('click', function (e) {
            e.preventDefault();
            Swal.fire({
                title: 'Sign out?',
                text: 'You will need to log in again to access the system.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, log me out',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#dc3545'
            }).then(function (result) {
                if (result.isConfirmed) {
                    document.getElementById('logoutForm').submit();
                }
            });
        });
    }

    // ------------------------------------------------------------
    // Real-time avatar preview (works for both Profile + Users forms)
    // ------------------------------------------------------------
    var avatarInput = document.getElementById('avatarInput');
    var avatarPreview = document.getElementById('avatarPreview');
    if (avatarInput && avatarPreview) {
        var defaultAvatar = avatarPreview.getAttribute('src');
        var avatarFileName = document.getElementById('avatarFileName');
        avatarInput.addEventListener('change', function () {
            var file = this.files && this.files[0];
            if (avatarFileName) avatarFileName.textContent = file ? file.name : 'No file selected';
            if (!file) {
                avatarPreview.src = defaultAvatar;
                return;
            }
            if (file.size > 3 * 1024 * 1024) {
                notyf.error('Image must be smaller than 3 MB.');
                avatarInput.value = '';
                return;
            }
            var reader = new FileReader();
            reader.onload = function (e) { avatarPreview.src = e.target.result; };
            reader.readAsDataURL(file);
        });
    }

    // ------------------------------------------------------------
    // Select2 (roles, country, department dropdowns)
    // ------------------------------------------------------------
    if (window.jQuery && jQuery.fn.select2) {
        initializeApplicationDropdowns(document);
        if (window.MutationObserver) {
            var applicationDropdownObserver = new MutationObserver(function (mutations) {
                mutations.forEach(function (mutation) {
                    mutation.addedNodes.forEach(function (node) {
                        if (node.nodeType === Node.ELEMENT_NODE && (node.matches('select') || node.querySelector('select'))) {
                            initializeApplicationDropdowns(node);
                        }
                    });
                });
            });
            applicationDropdownObserver.observe(document.body, { childList: true, subtree: true });
        }
    }

    // ------------------------------------------------------------
    // intl-tel-input (phone number fields) + real-time validation
    // ------------------------------------------------------------
    if (window.intlTelInput) {
        document.querySelectorAll('.phone-input').forEach(function (input) {
            const dialCode = input.dataset.contactCountryCode || '+92';
            const country = window.intlTelInput.getCountryData().find(function (item) {
                return '+' + item.dialCode === dialCode;
            });
            const iti = window.intlTelInput(input, {
                initialCountry: country ? country.iso2 : 'pk',
                separateDialCode: Boolean(input.dataset.countryCodeInput),
                utilsScript: 'https://cdn.jsdelivr.net/npm/intl-tel-input@25.3.1/build/js/utils.js'
            });

            const countryCodeInput = input.dataset.countryCodeInput
                ? document.getElementById(input.dataset.countryCodeInput)
                : null;
            function syncCountryCode() {
                if (countryCodeInput) {
                    const selectedCountry = iti.getSelectedCountryData();
                    countryCodeInput.value = selectedCountry.dialCode ? '+' + selectedCountry.dialCode : '+92';
                }
            }
            syncCountryCode();
            input.addEventListener('countrychange', syncCountryCode);

            const feedback = document.createElement('div');
            feedback.className = 'phone-feedback small mt-1';
            const wrapper = input.closest('.iti') || input;
            wrapper.insertAdjacentElement('afterend', feedback);

            function validate() {
                const value = input.value.trim();
                if (!value) {
                    input.classList.remove('is-valid', 'is-invalid');
                    feedback.textContent = '';
                    return;
                }
                if (iti.isValidNumber()) {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                    feedback.className = 'phone-feedback small mt-1 text-success';
                    feedback.textContent = '';
                } else {
                    input.classList.remove('is-valid');
                    input.classList.add('is-invalid');
                    feedback.className = 'phone-feedback small mt-1 text-danger';
                    feedback.textContent = 'Enter a valid phone number for the selected country.';
                }
            }

            input.addEventListener('blur', validate);
            input.addEventListener('input', function () {
                if (input.value.trim().length > 6) { validate(); }
                else { input.classList.remove('is-valid', 'is-invalid'); feedback.textContent = ''; }
            });
            input.addEventListener('countrychange', validate);
        });
    }

    // ------------------------------------------------------------
    // Flatpickr (custom date picker instead of browser default)
    // ------------------------------------------------------------
    if (window.flatpickr) {
        document.querySelectorAll('input.datepicker, input[type="date"]').forEach(function (el) {
            if (el._flatpickr) return;
            if (el.type === 'date') el.type = 'text';
            flatpickr(el, {
                dateFormat: 'Y-m-d',
                altInput: true,
                altFormat: 'd M Y',
                allowInput: true,
                maxDate: el.dataset.maxToday ? 'today' : null
            });
        });
    }

    // Keep invoice due dates in sync with the selected payment terms.
    document.querySelectorAll('[data-payment-terms]').forEach(function (termsSelect) {
        const form = termsSelect.closest('form');
        const invoiceDate = form && form.querySelector('[data-invoice-date]');
        const dueDate = form && form.querySelector('[data-due-date]');
        if (!invoiceDate || !dueDate) return;

        function recalculateDueDate() {
            if (termsSelect.value === '' || !invoiceDate.value) return;
            const parts = invoiceDate.value.slice(0, 10).split('-').map(Number);
            if (parts.length !== 3 || parts.some(Number.isNaN)) return;

            const date = new Date(parts[0], parts[1] - 1, parts[2]);
            date.setDate(date.getDate() + (parseInt(termsSelect.value, 10) || 0));
            const isoDate = [date.getFullYear(), String(date.getMonth() + 1).padStart(2, '0'), String(date.getDate()).padStart(2, '0')].join('-');
            if (dueDate._flatpickr) dueDate._flatpickr.setDate(isoDate, true);
            else dueDate.value = isoDate;
        }

        jQuery(termsSelect).on('change', recalculateDueDate);
        invoiceDate.addEventListener('change', recalculateDueDate);
    });

    // ------------------------------------------------------------
    // Generic bulk-import modal handler — works for every _ImportModal
    // instance in the app. Submits the file via fetch, shows a summary,
    // and fires an "erp:import-success" event so the hosting page can
    // reload its DataTable.
    // ------------------------------------------------------------
    document.querySelectorAll('.import-form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const url = form.getAttribute('data-action');
            const resultBox = form.querySelector('.import-result');
            const submitBtn = form.querySelector('button[type="submit"]');
            const formData = new FormData(form);

            resultBox.innerHTML = '';
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i> Importing...';

            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

            fetch(url, {
                method: 'POST',
                headers: tokenInput ? { 'RequestVerificationToken': tokenInput.value } : {},
                body: formData
            })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="fa-solid fa-upload me-1"></i> Import';

                    if (!data.success && data.errorCount === undefined) {
                        notyf.error(data.message || 'Import failed.');
                        return;
                    }

                    const alertCls = data.errorCount > 0 ? 'alert-warning' : 'alert-success';
                    let html = `<div class="alert ${alertCls} mb-0"><strong>${data.summary}</strong>`;
                    if (data.errors && data.errors.length) {
                        html += '<ul class="mb-0 mt-2 small">' + data.errors.slice(0, 10).map(function (e) { return `<li>${e}</li>`; }).join('') + '</ul>';
                        if (data.errors.length > 10) html += `<div class="small mt-1">...and ${data.errors.length - 10} more.</div>`;
                    }
                    html += '</div>';
                    resultBox.innerHTML = html;

                    notyf.success(data.summary);
                    document.dispatchEvent(new CustomEvent('erp:import-success', { detail: { formId: form.closest('.modal').id } }));

                    // Fully clean import (no row errors) — close the modal automatically
                    // so the user lands straight back on the refreshed grid. If there
                    // were row errors, leave it open so they can read and fix them.
                    if (data.errorCount === 0) {
                        var modalEl = form.closest('.modal');
                        var modalInstance = bootstrap.Modal.getOrCreateInstance(modalEl);
                        setTimeout(function () {
                            modalInstance.hide();
                            form.reset();
                            resultBox.innerHTML = '';
                        }, 1200);
                    }
                })
                .catch(function () {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = '<i class="fa-solid fa-upload me-1"></i> Import';
                    notyf.error('Import request failed. Please try again.');
                });
        });
    });


    // behaviour (with animate.css class toggling) on desktop, while
    // keeping click-to-open for touch devices.
    // ------------------------------------------------------------
    var isTouch = window.matchMedia('(hover: none)').matches;
    if (!isTouch) {
        document.querySelectorAll('.hoverable-dropdown').forEach(function (item) {
            var menu = item.querySelector('.dropdown-menu');
            var timeout;
            item.addEventListener('mouseenter', function () {
                clearTimeout(timeout);
                menu.classList.add('show', 'animate__animated', 'animate__fadeIn');
            });
            item.addEventListener('mouseleave', function () {
                timeout = setTimeout(function () { menu.classList.remove('show'); }, 150);
            });
        });
    }

    // ------------------------------------------------------------
    // DataTables default init for any table with class "data-table"
    // ------------------------------------------------------------
    if (window.jQuery && jQuery.fn.DataTable) {
        jQuery('.data-table').each(function () {
            if (!jQuery.fn.DataTable.isDataTable(this)) {
                jQuery(this).DataTable({
                    // responsive: true,
                    // pageLength: 10,
                    lengthMenu: [5, 10, 25, 50, 100]
                });
            }
        });
    }
});

// ============================================================
// Approval workflow helpers — used by Internal Transfer, Item
// Adjustment Note and Consumption Voucher list/detail pages.
// ============================================================
function approveVoucher(url, id, token, onSuccess) {
    Swal.fire({
        title: 'Approve this document?',
        text: 'It will be posted to the stock ledger immediately.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes, approve',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#16a34a'
    }).then(function (result) {
        if (!result.isConfirmed) return;

        fetch(url, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token, 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'id=' + encodeURIComponent(id)
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (data.success) {
                    notyf.success(data.message || 'Approved.');
                    if (typeof onSuccess === 'function') onSuccess();
                } else {
                    notyf.error(data.message || 'Could not approve.');
                }
            })
            .catch(function () { notyf.error('Request failed. Please try again.'); });
    });
}

function rejectVoucher(url, id, token, onSuccess) {
    Swal.fire({
        title: 'Reject this document?',
        input: 'textarea',
        inputLabel: 'Reason for rejection',
        inputPlaceholder: 'Explain why this is being rejected...',
        showCancelButton: true,
        confirmButtonText: 'Reject',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#ef4444',
        inputValidator: function (value) {
            if (!value) return 'A reason is required to reject a document.';
        }
    }).then(function (result) {
        if (!result.isConfirmed) return;

        fetch(url, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token, 'Content-Type': 'application/json' },
            body: JSON.stringify({ id: id, reason: result.value })
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (data.success) {
                    notyf.success(data.message || 'Rejected.');
                    if (typeof onSuccess === 'function') onSuccess();
                } else {
                    notyf.error(data.message || 'Could not reject.');
                }
            })
            .catch(function () { notyf.error('Request failed. Please try again.'); });
    });
}

function statusBadge(status) {
    const map = { Pending: 'badge-status-pending', Approved: 'badge-status-approved', Rejected: 'badge-status-rejected' };
    return `<span class="badge ${map[status] || 'bg-secondary'}">${status}</span>`;
}


// Usage: confirmDelete(url, token, function onSuccess(){ ... }, { id: 123 })
// ============================================================
function confirmDelete(url, token, onSuccess, formData) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#dc3545'
    }).then(function (result) {
        if (!result.isConfirmed) return;

        var body = null;
        var headers = { 'RequestVerificationToken': token };
        if (formData) {
            headers['Content-Type'] = 'application/x-www-form-urlencoded';
            body = Object.keys(formData).map(function (k) {
                return encodeURIComponent(k) + '=' + encodeURIComponent(formData[k]);
            }).join('&');
        }

        fetch(url, {
            method: 'POST',
            headers: headers,
            body: body
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (data.success) {
                    notyf.success(data.message || 'Deleted successfully.');
                    if (typeof onSuccess === 'function') onSuccess();
                } else {
                    notyf.error(data.message || 'Something went wrong.');
                }
            })
            .catch(function () { notyf.error('Request failed. Please try again.'); });
    });
}

// ============================================================
// Country -> City cascading dropdown (Select2, tags-enabled so
// any city not in the curated list can still be typed in).
// ============================================================
const COUNTRY_CITY_DATA = {
    "Pakistan": ["Karachi", "Lahore", "Islamabad", "Rawalpindi", "Faisalabad", "Multan", "Peshawar", "Sialkot", "Gujranwala", "Quetta"],
    "United States": ["New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "Austin"],
    "United Kingdom": ["London", "Manchester", "Birmingham", "Leeds", "Glasgow", "Liverpool", "Bristol", "Sheffield", "Edinburgh", "Newcastle"],
    "United Arab Emirates": ["Dubai", "Abu Dhabi", "Sharjah", "Ajman", "Al Ain", "Ras Al Khaimah", "Fujairah"],
    "Saudi Arabia": ["Riyadh", "Jeddah", "Mecca", "Medina", "Dammam", "Khobar", "Taif"],
    "India": ["Mumbai", "Delhi", "Bangalore", "Hyderabad", "Chennai", "Kolkata", "Pune", "Ahmedabad", "Jaipur", "Surat"],
    "China": ["Shanghai", "Beijing", "Guangzhou", "Shenzhen", "Chengdu", "Tianjin", "Wuhan", "Xi'an"],
    "Canada": ["Toronto", "Montreal", "Vancouver", "Calgary", "Edmonton", "Ottawa", "Winnipeg"],
    "Australia": ["Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Canberra"],
    "Germany": ["Berlin", "Hamburg", "Munich", "Cologne", "Frankfurt", "Stuttgart"],
    "France": ["Paris", "Marseille", "Lyon", "Toulouse", "Nice", "Nantes"],
    "Turkey": ["Istanbul", "Ankara", "Izmir", "Bursa", "Antalya"],
    "Bangladesh": ["Dhaka", "Chittagong", "Khulna", "Rajshahi", "Sylhet"],
    "Malaysia": ["Kuala Lumpur", "George Town", "Johor Bahru", "Ipoh", "Shah Alam"],
    "Singapore": ["Singapore"],
    "Qatar": ["Doha", "Al Rayyan", "Al Wakrah"],
    "Kuwait": ["Kuwait City", "Hawalli", "Al Ahmadi"],
    "Oman": ["Muscat", "Salalah", "Sohar"],
    "South Africa": ["Johannesburg", "Cape Town", "Durban", "Pretoria"],
    "Egypt": ["Cairo", "Alexandria", "Giza", "Shubra El Kheima"],
    "Other": []
};

function initCountryCityCascade(countrySelId, citySelId) {
    const countrySel = document.getElementById(countrySelId);
    const citySel = document.getElementById(citySelId);
    if (!countrySel || !citySel || !window.jQuery || !jQuery.fn.select2) return;

    // Country: curated list, but still typable (tags:true) for anything not listed.
    Object.keys(COUNTRY_CITY_DATA).forEach(function (country) {
        if (!countrySel.querySelector(`option[value="${CSS.escape(country)}"]`)) {
            const opt = document.createElement('option');
            opt.value = country;
            opt.textContent = country;
            countrySel.appendChild(opt);
        }
    });

    const existingCountry = countrySel.getAttribute('data-initial-value') || countrySel.value;
    const existingCity = citySel.getAttribute('data-initial-value') || citySel.value;

    jQuery(countrySel).select2({ theme: 'bootstrap-5', width: '100%', tags: true, placeholder: 'Select or type a country' });
    jQuery(citySel).select2({ theme: 'bootstrap-5', width: '100%', tags: true, placeholder: 'Select or type a city' });

    function populateCities(country, selectedCity) {
        const cities = COUNTRY_CITY_DATA[country] || [];
        jQuery(citySel).empty();
        if (selectedCity && !cities.includes(selectedCity)) cities.push(selectedCity);
        cities.forEach(function (city) {
            const opt = new Option(city, city, false, city === selectedCity);
            jQuery(citySel).append(opt);
        });
        jQuery(citySel).trigger('change');
    }

    if (existingCountry) {
        jQuery(countrySel).val(existingCountry).trigger('change.select2');
        populateCities(existingCountry, existingCity);
    }

    jQuery(countrySel).on('change', function () { populateCities(this.value, null); });
}

// ============================================================
// Multi-value tags field (e.g. multiple Customer PO Numbers) —
// a tags-enabled Select2 kept in sync with a single hidden input
// that actually gets posted/bound as a comma-separated string.
// ============================================================
function initTagsToHiddenField(hiddenInputId, tagsSelectId, placeholderText) {
    const hidden = document.getElementById(hiddenInputId);
    const tagsSelect = document.getElementById(tagsSelectId);
    if (!hidden || !tagsSelect || !window.jQuery || !jQuery.fn.select2) return;

    const initialValues = (hidden.value || '').split(',').map(function (v) { return v.trim(); }).filter(Boolean);
    initialValues.forEach(function (v) {
        const opt = new Option(v, v, true, true);
        tagsSelect.appendChild(opt);
    });

    jQuery(tagsSelect).select2({
        theme: 'bootstrap-5',
        width: '100%',
        tags: true,
        tokenSeparators: [',', ' '],
        placeholder: placeholderText || 'Type a value and press Enter'
    });

    jQuery(tagsSelect).on('change', function () {
        hidden.value = (jQuery(this).val() || []).join(', ');
    });
}
