(function ($) {
    'use strict';

    if (!$ || !$.fn || !$.fn.dataTable) {
        return;
    }

    var hasButtons = !!$.fn.dataTable.Buttons;
    var defaultButtons = [];

    if (hasButtons) {
        defaultButtons = [
            { extend: 'excelHtml5', className: 'btn btn-sm btn-primary', text: '<i class="fas fa-file-excel me-2"></i>Excel' },
            { extend: 'csvHtml5', className: 'btn btn-sm btn-outline-primary', text: '<i class="fas fa-file-csv me-2"></i>CSV' },
            { extend: 'pdfHtml5', className: 'btn btn-sm btn-outline-primary', text: '<i class="fas fa-file-pdf me-2"></i>PDF' }
        ];
    }

    var domLayout = "<'row align-items-center gy-2 mb-3'<'col-lg-4 col-md-6'l>";

    if (hasButtons) {
        domLayout += "<'col-lg-4 col-md-6 text-lg-center text-md-center mb-2 mb-md-0'B>";
    } else {
        domLayout += "<'col-lg-4 col-md-6 text-lg-center text-md-center mb-2 mb-md-0'>";
    }

    domLayout += "<'col-lg-4 col-md-12 text-lg-end text-md-end'f>>t" +
        "<'row align-items-center gy-2 mt-3'<'col-md-6'i><'col-md-6 text-md-end'p>>";

    $.extend(true, $.fn.dataTable.defaults, {
        responsive: true,
        autoWidth: false,
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        language: {
            search: '',
            searchPlaceholder: 'Search…',
            lengthMenu: '_MENU_',
            info: 'Showing _START_ to _END_ of _TOTAL_ entries',
            infoEmpty: 'No records available',
            infoFiltered: '(filtered from _MAX_ total entries)'
        },
        dom: domLayout,
        buttons: defaultButtons
    });

    $(document).on('init.dt', function (event, settings) {
        var api = new $.fn.dataTable.Api(settings);
        var container = $(api.table().container());

        container.find('div.dataTables_filter input')
            .addClass('form-control form-control-sm shadow-none')
            .attr('placeholder', 'Search records');

        container.find('div.dataTables_length select')
            .addClass('form-select form-select-sm shadow-none');

        container.find('div.dt-buttons .btn')
            .removeClass('dt-button')
            .addClass('shadow-none');
    });
})(window.jQuery);
