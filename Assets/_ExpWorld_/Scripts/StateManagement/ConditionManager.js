const within_subjects_variables_dummy = [
    { name: "color", values: ["R", "G", "B"], isRandom: false },
    { name: "size", values: [5, 10, 15], isRandom: true },
];

const between_subjects_variables_dummy = [
    { name: "method", values: ["old", "new"] }
];

$.onStart(() => {
    $.state.conditionDependentObjects = [];
    // $.state.betweenSubjectsCondition = {};
});

$.onUpdate(() => {
    if ($.getStateCompat("this", "exp_initCondition", "boolean")) {
        $.setStateCompat("this", "exp_initCondition", false);
        InitConditions();
    }
    if ($.state.conditions && !$.state.isLast && $.getStateCompat("global", "exp_conditionID", "integer") >= $.state.conditions.length - 1) {
        $.state.isLast = true;
        $.sendSignalCompat("this", "exp_readyToLeaveTrials");
    }
});

$.onReceive((messageType, arg, sender) => {
    switch (messageType) {
        case "exp_conditionDependentObject":
            $.state.conditionDependentObjects = [ ...$.state.conditionDependentObjects, sender ];
            break;
        case "exp_questionnaire_answer":
            // 参加者に参加者間条件を割り当てる
            InitBetweenSubjectCondition(arg);
        default:
            break;
    }
});

function InitConditions () {
    $.state.isLast = false;

    if (!$.state.betweenSubjectsCondition) {
        InitBetweenSubjectCondition(null);
    }
    
    try {
        $.state.conditions = getConditions(within_subjects_variables, trialsCountForEachUniqueCondition).map(cond => {
            return { ...cond, ...$.state.betweenSubjectsCondition };
        });
    } catch (e) {
        $.state.conditions = getConditions(within_subjects_variables_dummy).map(cond => {
            return { ...cond, ...$.state.betweenSubjectsCondition };
        });
    }
    
    $.log(JSON.stringify($.state.conditions));
    sendUpdates().catch(e => $.log(e));
}

async function sendUpdates() {
    let sentObjectsCount = 0;
    const sendPromises = $.state.conditionDependentObjects.map(async (obj, index) => {
        if (sentObjectsCount > 10) {
            await sleep(1000);
            sentObjectsCount = 0;
        }
        obj.send("exp_updateConditions", $.state.conditions);
        sentObjectsCount++;
    });
    await Promise.all(sendPromises);
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function InitBetweenSubjectCondition(questionnaireAnswers) {
    try {
        $.state.betweenSubjectsCondition = GetBetweenSubjectsCondition(questionnaireAnswers);
    }
    catch (e) {
        $.log("Function GetBetweenSubjectsCondition is not defined. Randomly assign between-subjects condition.");
        let betweenSubjectsCondition = {};
        try {
            between_subjects_variables.forEach(v => {
                betweenSubjectsCondition[v.name] = v.values[Math.floor(Math.random() * v.values.length)];
            });
        } catch (e) {
            $.log("Between-subjects variables are not defined. Use dummy variables.");
            between_subjects_variables_dummy.forEach(v => {
                betweenSubjectsCondition[v.name] = v.values[Math.floor(Math.random() * v.values.length)];
            });
        }
        $.state.betweenSubjectsCondition = betweenSubjectsCondition;
    }
}

// --------- Initialize conditions from variables ----------

function getConditions (variables, repeatPerCombination = 1) {
    const isRepetitionRandom = true;
    
    const allCombinations = generateCombinations(variables);
    
    // Group by non-random variables
    const groupedCombinations = {};
    
    for (const combination of allCombinations) {
        const groupKey = variables
            .filter(v => !v.isRandom)
            .map(v => combination[v.name])
            .join('-');
    
        if (!groupedCombinations[groupKey]) {
            groupedCombinations[groupKey] = [];
        }
        groupedCombinations[groupKey].push(combination);
    }
    
    // Shuffle each group
    for (const key in groupedCombinations) {
        groupedCombinations[key] = shuffleArray(groupedCombinations[key]);
    }
    
    // Flatten the grouped combinations into a single array
    let finalCombinations = Object.values(groupedCombinations).flat();
    
    // Repeat each combination repeatPerCombination times
    let conditions = [];
    for (const combination of finalCombinations) {
        for (let i = 0; i < repeatPerCombination; i++) {
            conditions.push({ ...combination });
        }
    }
    
    // If repetition should be random, shuffle the repeated combinations
    if (isRepetitionRandom) {
        // Group by non-random variables first
        const randomGroupedCombinations = {};
        for (const combination of conditions) {
            const groupKey = variables
                .filter(v => !v.isRandom)
                .map(v => combination[v.name])
                .join('-');
    
            if (!randomGroupedCombinations[groupKey]) {
                randomGroupedCombinations[groupKey] = [];
            }
            randomGroupedCombinations[groupKey].push(combination);
        }
    
        // Shuffle within each group
        for (const key in randomGroupedCombinations) {
            randomGroupedCombinations[key] = shuffleArray(randomGroupedCombinations[key]);
        }
    
        // Flatten the grouped combinations into a single array
        conditions = Object.values(randomGroupedCombinations).flat();
    }
    
    return conditions;
}

function shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
}

function generateCombinations(variables) {
    const combinations = [];

    function createCombination(currentCombination, currentIndex) {
        if (currentIndex === variables.length) {
            combinations.push({ ...currentCombination });
            return;
        }

        const variable = variables[currentIndex];
        for (const value of variable.values) {
            currentCombination[variable.name] = value;
            createCombination(currentCombination, currentIndex + 1);
        }
    }

    createCombination({}, 0);

    return combinations;
}
