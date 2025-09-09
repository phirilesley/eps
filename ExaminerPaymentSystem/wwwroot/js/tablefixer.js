  // Get all elements with class "fee" and "quantity"
  const fees = document.querySelectorAll('.fee');
  const quantities = document.querySelectorAll('.quantity');

  // Add event listener to each quantity input
  quantities.forEach(quantity => {
      quantity.addEventListener('input', updateTotal);
  });

  // Function to update total
  function updateTotal() {
      let total = 0;
      // Iterate through each fee and quantity input
      fees.forEach((fee, index) => {
          const quantity = quantities[index].value || 0; // If quantity is empty, default to 0
          total += parseFloat(fee.value || 0) * parseFloat(quantity); // Multiply fee with quantity and add to total
      });
      // Update the total in the HTML
      document.getElementById('total').textContent = '$' + total.toFixed(2);

  }

  function calculateRowTotal(index) {
      alert("In function");
      var busfare = parseFloat(document.getElementsByName('TANDSDETAILS[' + index + '].EMS_BUSFARE')[0].value) || 0;
      var accommodation = parseFloat(document.getElementsByName('TANDSDETAILS[' + index + '].EMS_ACCOMMODATION')[0].value) || 0;
      var lunch = parseFloat(document.getElementsByName('TANDSDETAILS[' + index + '].EMS_LUNCH')[0].value) || 0;
      var dinner = parseFloat(document.getElementsByName('TANDSDETAILS[' + index + '].EMS_DINNER')[0].value) || 0;

      var total = busfare + accommodation + lunch + dinner;

      document.getElementsByName('TANDSDETAILS[' + index + '].EMS_TOTAL')[0].value = total.toFixed(2);

      // Update total expenses
      updateTotalExpenses();
  }

function calculateRowTotal() {
    let totalBusfare = 0;
    let totalAccommodation = 0;
    let totalLunch = 0;
    let totalDinner = 0;

    $('tbody tr').each(function () {
        let busfare = parseFloat($(this).find('.busfare-value').val()) || 0;
        let accommodation = parseFloat($(this).find('.accommodation-value').val()) || 0;
        let lunch = parseFloat($(this).find('.lunch-value').val()) || 0;
        let dinner = parseFloat($(this).find('.dinner-value').val()) || 0;

        console.log('Busfare:', busfare);
        console.log('Accommodation:', accommodation);
        console.log('Lunch:', lunch);
        console.log('Dinner:', dinner);

        totalBusfare += busfare;
        totalAccommodation += accommodation;
        totalLunch += lunch;
        totalDinner += dinner;

        let total = busfare + accommodation + lunch + dinner;
        $(this).find('.total-value').val(total.toFixed(2));

        console.log('Total:', total);
       
    });

  
    console.log('Total Busfare:', totalBusfare);
    console.log('Total Accommodation:', totalAccommodation);
    console.log('Total Lunch:', totalLunch);
    console.log('Total Dinner:', totalDinner);

    $('#totalBusfare').text(totalBusfare.toFixed(2));
    $('#totalAccommodation').text(totalAccommodation.toFixed(2));
    $('#totalLunch').text(totalLunch.toFixed(2));
    $('#totalDinner').text(totalDinner.toFixed(2));

    let grandTotal = totalBusfare + totalAccommodation + totalLunch + totalDinner;
    $('#grandTotal').text(grandTotal.toFixed(2));
    $('#grandTotal2').text(grandTotal.toFixed(2));
}




function closePage() {
    // close the current page
    window.close();
}

var finishedProcessing = false;

// Listen for a certain command or event (e.g., click on a button)
function animateBarRacho() {
    const container = document.getElementById('progress-bar-container');
    document.getElementById('progress-bar-container').style.display = 'block';
    const bar = new ProgressBar.Circle(container, {
        strokeWidth: 6,
        color: '#FF5722', // Set the color of the progress bar 
        trailColor: '#f4f4f4', // Set the color of the trail 
        trailWidth: 6, duration: 2000, // Set the duration for the animation 
        easing: 'easeInOut' // Set the easing function 
    });

    // Function to start the progress bar animation 
    function startProgressBar() {
        bar.animate(1);
        // Animate to 100% progress 
    }

    // Function to loop the progress bar animation 
    function loopProgressBar() {

        
        bar.set(0);

        // Reset the progress bar 
        startProgressBar(); // Start the progress bar animation 
        setTimeout(loopProgressBar, 2000);
        // Call the function again after 2 seconds 
    }
    
    if (finishedProcessing == false) {
        loopProgressBar(); // Start the loop
    }
    else {
        bar.stop();
       
    }
}