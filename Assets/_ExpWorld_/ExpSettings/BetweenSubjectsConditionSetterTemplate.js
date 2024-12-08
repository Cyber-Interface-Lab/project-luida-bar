function GetBetweenSubjectsCondition(questionnaireAnswers) {
    let betweenSubjectsCondition = {};
    try {
        between_subjects_variables.forEach(v => {
            // The line below randomly assigns between-subjects condition. Comment out this line and add your implementation.
            betweenSubjectsCondition[v.name] = v.values[Math.floor(Math.random() * v.values.length)];
            
            // Add your implementation here to assign between-subjects condition based on questionnaire answers.
            
        });
    } catch (e) {
        $.log("Between-subjects variables are not defined. Use dummy variables.");
        try {
            between_subjects_variables_dummy.forEach(v => {
                betweenSubjectsCondition[v.name] = v.values[Math.floor(Math.random() * v.values.length)];
            });
        } catch (e) {
            $.log("Dummy between-subjects variables are not found. Return empty object");
        }
    }
    return betweenSubjectsCondition;
}