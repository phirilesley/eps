function calculateNextTotal(input, iteratoryangu) {
    // Get all input fields with names starting with "TANDSDETAILS[iteratoryangu]" within the parent table
    var parentTable = input.closest('table');

    /*------------------------------------------------CODE TO CHECK DOUBLE CAPTURE OF FIELDS ON SAME DATE---------------------------------------------------------------------*/
    var dateField = parentTable.querySelector(`input[name^="TANDSDETAILS[${iteratoryangu}"].EMS_DATE`);
    var previousDateField = parentTable.querySelector(`input[name^="TANDSDETAILS[${iteratoryangu-1}"].EMS_DATE`);
    var accomodationField = parentTable.querySelector(`input[name^="TANDSDETAILS[${iteratoryangu}"].EMS_ACCOMMODATION`);
    var lunchField = parentTable.querySelector(`input[name^="TANDSDETAILS[${iteratoryangu}"].EMS_LUNCH`);
    var accomodationField = parentTable.querySelector(`input[name^="TANDSDETAILS[${iteratoryangu}"].EMS_DINNER`);
   /*-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
    



    // Initialize total sum
    var inputs = parentTable.querySelectorAll(`input[name^="TANDSDETAILS[${iteratoryangu}"]:not([name$="EMS_TOTAL"]):not([name$="EMS_PLACE"]):not([name$="EMS_ARRIVAL"]):not([name$="EMS_DEPARTURE"]):not([name$="EMS_DATE"])`);
    var total = 0;
    // Iterate over each input field and sum up the values
    inputs.forEach(function (inputField) {
        total += parseFloat(inputField.value) || 0; // Parse input value to float or use 0 if NaN
    });
    // Update the value of TANDSDETAILS[0].EMS_TOTAL input field with the calculated total
    var totalInput = document.getElementById("TANDSDETAILS[" + iteratoryangu + "].EMS_TOTAL");
    totalInput.value = total.toFixed(2); // Round the total to 2 decimal places and set it as the input value


    const inputFields = Array.from(document.querySelectorAll('[name^="TANDSDETAILS"][name$=".EMS_TOTAL"]'));

    const totalSpan = document.getElementById('totala');
    const totalSpanB = document.getElementById('total');
    const grandTotalSpan = document.getElementById('totalF');

    let totalValues = [];

    inputFields.forEach((input) => {
        totalValues.push(parseFloat(input.value));
    });

    let totalValueA = 0.00;
    totalValues.forEach((value) => {
        totalValueA += value;
    });

    totalSpan.textContent = `$${totalValueA.toFixed(2)}`;

    let grandTotalText = parseFloat(totalSpanB.textContent.replace(/[^0-9\.]/g, '')) + totalValueA;
    grandTotalSpan.textContent = `$${grandTotalText.toFixed(2)}`;
}

function addRow() {
    var table = document.getElementById("myTable");
    var rowNumber = table.rows.length - 1;
    var newRow = table.insertRow(-1);

    newRow.innerHTML = `
        <td><input type="date" name="TANDSDETAILS[${rowNumber}].EMS_DATE" class="form-control form-control-sm"></td>
        <td><input name="TANDSDETAILS[${rowNumber}].EMS_DEPARTURE" class="form-control form-control-sm"></td>
        <td><input type="text" name="TANDSDETAILS[${rowNumber}].EMS_ARRIVAL" class="form-control form-control-sm"></td>
        <td><input name="TANDSDETAILS[${rowNumber}].EMS_PLACE" class="form-control form-control-sm"></td>
        <td><input type="number" step="any" name="TANDSDETAILS[${rowNumber}].EMS_BUSFARE" oninput="calculateNextTotal(this, ${rowNumber})" class="form-control form-control-sm" ></td>
        <td><input type="number" step="any" name="TANDSDETAILS[${rowNumber}].EMS_ACCOMMODATION" oninput="calculateNextTotal(this, ${rowNumber})" class="form-control form-control-sm" ></td>
        <td><input type="number" step="any" name="TANDSDETAILS[${rowNumber}].EMS_LUNCH" oninput="calculateNextTotal(this, ${rowNumber})" class="form-control form-control-sm" ></td>
        <td><input type="number" step="any" name="TANDSDETAILS[${rowNumber}].EMS_DINNER" oninput="calculateNextTotal(this, ${rowNumber})" class="form-control form-control-sm" ></td>
        <td><input type="number" step="any" name="TANDSDETAILS[${rowNumber}].EMS_TOTAL" id="TANDSDETAILS[${rowNumber}].EMS_TOTAL" readonly class="form-control form-control-sm"></td>
        <td>
            <div class="d-flex">
                <button class="btn btn-success btn-sm me-1" type="button" onclick="addRow()">
                    <i class="fas fa-plus-circle"></i>
                </button>
                <button class="btn btn-danger btn-sm" type="button" onclick="deleteRow(this)">
                    <i class="fas fa-trash-alt"></i>
                </button>
            </div>
        </td>`;
}





function deleteRow(button) {
    // Find the row that contains the button
    var row = button.closest('tr');

    if (!row) {
        console.error("Row not found");
        return;
    }

    // Update total values
    var totalSpan = document.getElementById('totalF');
    var totalSpanB = document.getElementById('totala');

    // Check if totalSpan elements exist
    if (!totalSpan || !totalSpanB) {
        console.error("Total elements not found");
        return;
    }

    var totalValue = parseFloat(totalSpan.textContent.replace(/[^0-9.-]/g, '')) || 0;
    var totalValueB = parseFloat(totalSpanB.textContent.replace(/[^0-9.-]/g, '')) || 0;

    // Use more specific query selector to find the input within the row
    var totalInput = row.querySelector('input[name$=".EMS_TOTAL"]');
    if (!totalInput) {
        console.error("Total input not found in the row");
        return;
    }

    var value = parseFloat(totalInput.value) || 0;
    totalValue -= value;
    totalValueB -= value;

    // Update the total values in the respective spans
    totalSpan.textContent = `$${totalValue.toFixed(2)}`;
    totalSpanB.textContent = `$${totalValueB.toFixed(2)}`;

    // Remove the row from the table
    row.parentNode.removeChild(row);
}


function calculateTotal(input) {
    // Get all input fields with names starting with "TANDSDETAILS[0]" within the parent table
    var parentTable = input.closest('table');
    var inputs = parentTable.querySelectorAll('input[name^="TANDSDETAILS[0]"]:not([name="TANDSDETAILS[0].EMS_TOTAL"]):not([name="TANDSDETAILS[0].EMS_PLACE"]):not([name="TANDSDETAILS[0].EMS_ARRIVAL"]):not([name="TANDSDETAILS[0].EMS_DEPARTURE"]):not([name="TANDSDETAILS[0].EMS_DATE"])');
    // Initialize total sum
    var total = 0;

    // Iterate over each input field and sum up the values
    inputs.forEach(function (inputField) {
        total += parseFloat(inputField.value) || 0; // Parse input value to float or use 0 if NaN
    });
    //alert("new total value is : " + total);
    // Update the value of TANDSDETAILS[0].EMS_TOTAL input field with the calculated total
    var totalInput = document.getElementById("TANDSDETAILS[0].EMS_TOTAL");
    totalInput.value = total.toFixed(2);

    const inputFields = Array.from(document.querySelectorAll('[name^="TANDSDETAILS"][name$=".EMS_TOTAL"]'));

    const totalSpan = document.getElementById('totala');
    const totalSpanB = document.getElementById('total');
    const grandTotalSpan = document.getElementById('totalF');

    let totalValues = [];

    inputFields.forEach((input) => {
        totalValues.push(parseFloat(input.value));
    });

    let totalValueA = 0.00;
    totalValues.forEach((value) => {
        totalValueA += value;
    });

    totalSpan.textContent = `$${totalValueA.toFixed(2)}`;

    let grandTotalText = parseFloat(totalSpanB.textContent.replace(/[^0-9\.]/g, '')) + totalValueA;
    grandTotalSpan.textContent = `$${grandTotalText.toFixed(2)}`;
}

function redoTotals() {
    const totalSpan = document.getElementById('totala');
    const totalSpanB = document.getElementById('total');
    const grandTotalSpan = document.getElementById('totalF');

    let grandTotalText = parseFloat(totalSpan.textContent.replace(/[^0-9\.]/g, '')) + parseFloat(totalSpanB.textContent.replace(/[^0-9\.]/g, ''));
    grandTotalSpan.textContent = `$${grandTotalText.toFixed(2)}`;
}

function checkFirst() {
    var firstRowValue = document.getElementById("TANDSDETAILS[0].EMS_TOTAL");

    if (firstRowValue.value.trim() == "") {
        Swal.fire({
            title: "Empty fields!",
            text: "Some cells have empty values. Fill those fields and try again.",
            icon: "error"
        });
        return;
    }
    else {
        addRow();
    }   
}