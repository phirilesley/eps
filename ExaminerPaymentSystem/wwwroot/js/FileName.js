
$(document).ready(function () {
    // Function to populate subject dropdown based on selected exam code
    $('#ExamCode').change(function () {
        console.log("Exam code selection changed");
        var selectedExamCode = $(this).val();
        console.log("Selected exam code: " + selectedExamCode);
        var subjectSelect = $('#Subject');
        subjectSelect.empty().append($('<option></option>').attr('value', '').text('-- Select Subject --'));

        // Filter subjects based on selected exam code and remove duplicates
        var uniqueSubjects = {};
        var examCodePrefix = '@ViewBag.SelectedExamCode'; // Access selectedExamCode from ViewBag
        console.log("Exam code prefix from ViewBag: " + examCodePrefix);
        @foreach(var subject in subjects)
    {
        var subSubIdPrefix = subject.SUB_SUB_ID.substring(0, 3);
        console.log("Subject SUB_SUB_ID prefix: " + subSubIdPrefix);
        if (subSubIdPrefix === examCodePrefix) {
            uniqueSubjects[subject.SUB_SUB_ID] = subject;
            console.log("Added subject: " + subject.SUB_SUB_ID);
        }
    }

    // Populate subject dropdown with unique subjects
    Object.values(uniqueSubjects).forEach(function (subject) {
        var optionText = subject.SUB_SUB_ID + '-' + subject.SUB_SUBJECT_DESC;
        subjectSelect.append($('<option></option>').attr('value', subject.SUB_SUB_ID).text(optionText));
        console.log("Appended subject: " + subject.SUB_SUB_ID);
    });
});
    });

