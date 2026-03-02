// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function($){
  function suppressClientValidationMessages() {
    if (!window.jQuery || !$.validator) return;
    if ($.validator.messages) $.validator.messages.required = '';
    if (typeof $.validator.setDefaults === 'function') {
      $.validator.setDefaults({
        errorPlacement: function() {},
        success: function(label) { if (label && label.remove) label.remove(); }
      });
    }
  }

  function initVehicleFormUx() {
    var form = document.getElementById('vehicleFormStandalone');
    var saveBtn = document.getElementById('btnSaveStandalone');
    if (!form || !saveBtn) return;

    var nowYear = new Date().getFullYear();
    var licensePlatePattern = /^(?:\d{2}-[A-Z]{2}-\d{2}|[A-Z]{2}-\d{2}-\d{2}|\d{2}-\d{2}-[A-Z]{2}|[A-Z]{2}-\d{2}-[A-Z]{2})$/;
    var fields = [
      { id: 'brand', missing: 'Obrigatorio.' },
      { id: 'model', missing: 'Obrigatorio.' },
      { id: 'licensePlate', missing: 'Obrigatorio.', invalid: 'Formato invalido.' },
      { id: 'year', missing: 'Obrigatorio.', invalid: 'Ano entre 1901 e ' + nowYear + '.' },
      { id: 'fuel', missing: 'Obrigatorio.' }
    ].map(function(cfg){
      var el = form.querySelector('#' + cfg.id);
      var field = el ? el.closest('.vehicle-field') : null;
      var hint = field ? field.querySelector('[data-field-hint-for="' + cfg.id + '"]') : null;
      return { cfg: cfg, el: el, field: field, hint: hint, touched: false };
    }).filter(function(item){ return !!item.el; });

    var submitted = false;

    function isLetter(ch) {
      return /^[A-Z]$/.test(ch);
    }

    function isDigit(ch) {
      return /^[0-9]$/.test(ch);
    }

    function isSameType(a, b) {
      return (isLetter(a) && isLetter(b)) || (isDigit(a) && isDigit(b));
    }

    function canAppendPlateChar(current, ch) {
      var n = current.length;
      if (n >= 6) return false;

      // Segment 1 (pos 0-1): both chars must have same type (AA or 11)
      if (n === 0) return isLetter(ch) || isDigit(ch);
      if (n === 1) return isSameType(current[0], ch);

      var firstTypeIsLetter = isLetter(current[0]);

      // Segment 2 (pos 2-3)
      // If first segment is letters, second must be digits.
      // If first segment is digits, second can be letters or digits.
      if (n === 2) {
        return firstTypeIsLetter ? isDigit(ch) : (isLetter(ch) || isDigit(ch));
      }
      if (n === 3) {
        return isSameType(current[2], ch);
      }

      var secondTypeIsLetter = isLetter(current[2]);

      // Segment 3 (pos 4-5)
      // Valid families:
      // AA-11-(AA|11), 11-AA-11, 11-11-AA
      if (n === 4) {
        if (firstTypeIsLetter && !secondTypeIsLetter) return isLetter(ch) || isDigit(ch);
        if (!firstTypeIsLetter && secondTypeIsLetter) return isDigit(ch);
        if (!firstTypeIsLetter && !secondTypeIsLetter) return isLetter(ch);
        return false;
      }
      if (n === 5) {
        return isSameType(current[4], ch);
      }

      return false;
    }

    function formatLicensePlateValue(value) {
      var raw = (value || '')
        .toString()
        .toUpperCase()
        .replace(/[^A-Z0-9]/g, '');

      var cleaned = '';
      for (var i = 0; i < raw.length; i++) {
        var ch = raw.charAt(i);
        if (canAppendPlateChar(cleaned, ch)) cleaned += ch;
        if (cleaned.length >= 6) break;
      }

      var p1 = cleaned.slice(0, 2);
      var p2 = cleaned.slice(2, 4);
      var p3 = cleaned.slice(4, 6);
      var formatted = '';

      if (p1) formatted += p1;
      if (p1.length === 2 && cleaned.length < 3) return formatted + '-';

      if (p2) formatted += '-' + p2;
      if (p2.length === 2 && cleaned.length < 5) return formatted + '-';

      if (p3) formatted += '-' + p3;
      return formatted;
    }

    function setState(item, state, message) {
      if (!item.field) return;
      item.field.classList.remove('field-warning', 'field-ok');
      if (state === 'warning') item.field.classList.add('field-warning');
      if (state === 'ok') item.field.classList.add('field-ok');
      if (item.hint && message) item.hint.textContent = message;
      if (state === 'warning') {
        item.el.classList.remove('is-valid');
        item.el.classList.add('is-invalid');
      } else if (state === 'ok') {
        item.el.classList.remove('is-invalid');
        item.el.classList.add('is-valid');
      } else {
        item.el.classList.remove('is-invalid');
        item.el.classList.remove('is-valid');
      }
    }

    function valueMissing(el) {
      if (!el) return true;
      var tag = (el.tagName || '').toLowerCase();
      if (tag === 'select') return !el.value || el.value === '';
      return !el.value || el.value.toString().trim() === '';
    }

    function yearInvalid(el) {
      if (!el || valueMissing(el)) return false;
      var year = parseInt(el.value, 10);
      return Number.isNaN(year) || year < 1901 || year > nowYear;
    }

    function licensePlateInvalid(el) {
      if (!el || valueMissing(el)) return false;
      var value = (el.value || '').toString().trim().toUpperCase();
      return !licensePlatePattern.test(value);
    }

    function evaluate(item) {
      if (!item.el) return true;
      var shouldShow = item.touched || submitted;
      if (!shouldShow) {
        setState(item, 'neutral', '');
        return !valueMissing(item.el);
      }

      if (valueMissing(item.el)) {
        setState(item, 'warning', item.cfg.missing);
        return false;
      }

      if (item.cfg.id === 'year' && yearInvalid(item.el)) {
        setState(item, 'warning', item.cfg.invalid);
        return false;
      }

      if (item.cfg.id === 'licensePlate' && licensePlateInvalid(item.el)) {
        setState(item, 'warning', item.cfg.invalid);
        return false;
      }

      setState(item, 'ok', 'OK');
      return true;
    }

    function refreshButtonState() {
      var valid = true;
      fields.forEach(function(item){
        var itemValid = evaluate(item);
        if (!itemValid) valid = false;
      });
      saveBtn.disabled = !valid;
    }

    fields.forEach(function(item){
      ['blur', 'input', 'change'].forEach(function(evt){
        item.el.addEventListener(evt, function(){
          if (item.cfg.id === 'licensePlate') {
            item.el.value = formatLicensePlateValue(item.el.value);
          }
          item.touched = true;
          refreshButtonState();
        });
      });
    });

    var plateItem = fields.find(function(item){ return item.cfg.id === 'licensePlate'; });
    if (plateItem && plateItem.el) {
      plateItem.el.value = formatLicensePlateValue(plateItem.el.value);
    }

    form.addEventListener('submit', function(e){
      submitted = true;
      fields.forEach(function(item){ item.touched = true; });
      refreshButtonState();

      if (saveBtn.disabled) {
        e.preventDefault();
        var firstInvalid = fields.find(function(item){
          return item.el && item.el.classList.contains('is-invalid');
        });
        if (firstInvalid && firstInvalid.el && typeof firstInvalid.el.focus === 'function') {
          firstInvalid.el.focus();
        }
      }
    });

    refreshButtonState();
  }

  function initClientFormUx() {
    var form = document.getElementById('clientFormStandalone');
    var saveBtn = document.getElementById('btnSaveClientStandalone');
    if (!form || !saveBtn) return;

    var phonePattern = /^\+\d{1,3}\d{9}$/;
    var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    var fields = [
      { id: 'clientName', missing: 'Obrigatorio.' },
      { id: 'clientEmail', missing: 'Obrigatorio.', invalid: 'Email invalido.' },
      { id: 'clientPhoneNumber', missing: 'Obrigatorio.', invalid: 'Formato: +351912345678' },
      { id: 'clientDriverLicense', missing: 'Obrigatorio.' }
    ].map(function(cfg){
      var el = form.querySelector('#' + cfg.id);
      var field = el ? el.closest('.vehicle-field') : null;
      var hint = field ? field.querySelector('[data-field-hint-for="' + cfg.id + '"]') : null;
      return { cfg: cfg, el: el, field: field, hint: hint, touched: false };
    }).filter(function(item){ return !!item.el; });

    var submitted = false;

    function setState(item, state, message) {
      if (!item.field) return;
      item.field.classList.remove('field-warning', 'field-ok');
      if (state === 'warning') item.field.classList.add('field-warning');
      if (state === 'ok') item.field.classList.add('field-ok');
      if (item.hint && message) item.hint.textContent = message;
      if (state === 'warning') {
        item.el.classList.remove('is-valid');
        item.el.classList.add('is-invalid');
      } else if (state === 'ok') {
        item.el.classList.remove('is-invalid');
        item.el.classList.add('is-valid');
      } else {
        item.el.classList.remove('is-invalid');
        item.el.classList.remove('is-valid');
      }
    }

    function valueMissing(el) {
      return !el || !el.value || el.value.toString().trim() === '';
    }

    function isInvalid(item) {
      if (item.cfg.id === 'clientEmail') {
        return !emailPattern.test((item.el.value || '').toString().trim());
      }
      if (item.cfg.id === 'clientPhoneNumber') {
        return !phonePattern.test((item.el.value || '').toString().trim());
      }
      return false;
    }

    function evaluate(item) {
      var shouldShow = item.touched || submitted;
      if (!shouldShow) {
        setState(item, 'neutral', '');
        return !valueMissing(item.el);
      }

      if (valueMissing(item.el)) {
        setState(item, 'warning', item.cfg.missing);
        return false;
      }

      if (isInvalid(item)) {
        setState(item, 'warning', item.cfg.invalid);
        return false;
      }

      setState(item, 'ok', 'OK');
      return true;
    }

    function refreshButtonState() {
      var valid = true;
      fields.forEach(function(item){
        var itemValid = evaluate(item);
        if (!itemValid) valid = false;
      });
      saveBtn.disabled = !valid;
    }

    fields.forEach(function(item){
      ['blur', 'input', 'change'].forEach(function(evt){
        item.el.addEventListener(evt, function(){
          item.touched = true;
          refreshButtonState();
        });
      });
    });

    form.addEventListener('submit', function(e){
      submitted = true;
      fields.forEach(function(item){ item.touched = true; });
      refreshButtonState();

      if (saveBtn.disabled) {
        e.preventDefault();
        var firstInvalid = fields.find(function(item){
          return item.el && item.el.classList.contains('is-invalid');
        });
        if (firstInvalid && firstInvalid.el && typeof firstInvalid.el.focus === 'function') {
          firstInvalid.el.focus();
        }
      }
    });

    refreshButtonState();
  }

  function initRevealAnimations() {
    var items = Array.prototype.slice.call(document.querySelectorAll('[data-reveal]'));
    if (!items.length) return;

    if (!('IntersectionObserver' in window)) {
      items.forEach(function(item){ item.classList.add('is-visible'); });
      return;
    }

    var observer = new IntersectionObserver(function(entries){
      entries.forEach(function(entry){
        if (!entry.isIntersecting) return;
        entry.target.classList.add('is-visible');
        observer.unobserve(entry.target);
      });
    }, { threshold: 0.12 });

    items.forEach(function(item){ observer.observe(item); });
  }

  function initRentalContractFormUx() {
    var form = document.getElementById('rentalContractFormStandalone');
    var saveBtn = document.getElementById('btnSaveRentalContractStandalone');
    if (!form || !saveBtn) return;

    var today = new Date();
    today.setHours(0, 0, 0, 0);

    var fields = [
      { id: 'contractClientId', missing: 'Obrigatorio.' },
      { id: 'contractVehicleId', missing: 'Obrigatorio.' },
      { id: 'rentalStartDate', missing: 'Obrigatorio.', invalid: 'Nao pode ser anterior a hoje.' },
      { id: 'rentalEndDate', missing: 'Obrigatorio.', invalid: 'Tem de ser posterior a data de inicio.' },
      { id: 'initialMileage', missing: 'Obrigatorio.', invalid: 'Tem de ser 0 ou superior.' }
    ].map(function(cfg){
      var el = form.querySelector('#' + cfg.id);
      var field = el ? el.closest('.vehicle-field') : null;
      var hint = field ? field.querySelector('[data-field-hint-for="' + cfg.id + '"]') : null;
      return { cfg: cfg, el: el, field: field, hint: hint, touched: false };
    }).filter(function(item){ return !!item.el; });

    var submitted = false;

    function setState(item, state, message) {
      if (!item.field) return;
      item.field.classList.remove('field-warning', 'field-ok');
      if (state === 'warning') item.field.classList.add('field-warning');
      if (state === 'ok') item.field.classList.add('field-ok');
      if (item.hint && message) item.hint.textContent = message;

      if (state === 'warning') {
        item.el.classList.remove('is-valid');
        item.el.classList.add('is-invalid');
      } else if (state === 'ok') {
        item.el.classList.remove('is-invalid');
        item.el.classList.add('is-valid');
      } else {
        item.el.classList.remove('is-invalid');
        item.el.classList.remove('is-valid');
      }
    }

    function valueMissing(el) {
      if (!el) return true;
      var tag = (el.tagName || '').toLowerCase();
      if (tag === 'select') return !el.value || el.value === '';
      return !el.value || el.value.toString().trim() === '';
    }

    function parseDate(value) {
      var d = new Date(value);
      if (Number.isNaN(d.getTime())) return null;
      d.setHours(0, 0, 0, 0);
      return d;
    }

    function isInvalid(item) {
      if (item.cfg.id === 'rentalStartDate') {
        var start = parseDate(item.el.value);
        return !start || start < today;
      }

      if (item.cfg.id === 'rentalEndDate') {
        var end = parseDate(item.el.value);
        var startDateInput = form.querySelector('#rentalStartDate');
        var startDate = startDateInput ? parseDate(startDateInput.value) : null;
        return !end || !startDate || end <= startDate;
      }

      if (item.cfg.id === 'initialMileage') {
        var km = parseInt(item.el.value, 10);
        return Number.isNaN(km) || km < 0;
      }

      return false;
    }

    function evaluate(item) {
      var shouldShow = item.touched || submitted;
      if (!shouldShow) {
        setState(item, 'neutral', '');
        return !valueMissing(item.el);
      }

      if (valueMissing(item.el)) {
        setState(item, 'warning', item.cfg.missing);
        return false;
      }

      if (isInvalid(item)) {
        setState(item, 'warning', item.cfg.invalid);
        return false;
      }

      setState(item, 'ok', 'OK');
      return true;
    }

    function refreshButtonState() {
      var valid = true;
      fields.forEach(function(item){
        var itemValid = evaluate(item);
        if (!itemValid) valid = false;
      });
      saveBtn.disabled = !valid;
    }

    fields.forEach(function(item){
      ['blur', 'input', 'change'].forEach(function(evt){
        item.el.addEventListener(evt, function(){
          item.touched = true;
          refreshButtonState();
        });
      });
    });

    form.addEventListener('submit', function(e){
      submitted = true;
      fields.forEach(function(item){ item.touched = true; });
      refreshButtonState();

      if (saveBtn.disabled) {
        e.preventDefault();
        var firstInvalid = fields.find(function(item){
          return item.el && item.el.classList.contains('is-invalid');
        });
        if (firstInvalid && firstInvalid.el && typeof firstInvalid.el.focus === 'function') {
          firstInvalid.el.focus();
        }
      }
    });

    refreshButtonState();
  }

  $(function(){
    suppressClientValidationMessages();
    initVehicleFormUx();
    initClientFormUx();
    initRentalContractFormUx();
    initRevealAnimations();
  });
})(jQuery);
