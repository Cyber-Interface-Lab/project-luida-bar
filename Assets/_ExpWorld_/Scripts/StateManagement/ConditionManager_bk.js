const variables_dummy = [
    { name: "color", values: ["R", "G", "B"], isRandom: false },
    { name: "size", values: [5, 10, 15], isRandom: true },
];

const trialsCountForEachCondition = 1;

$.onStart(() => {
    $.state.conditionDependentObjects = [];
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
        // case "exp_initCondition":
        //     InitConditions();
        //     break;
        default:
            break;
    }
});

function InitConditions () {
    $.state.isLast = false;

    $.state.conditions = getConditions(variables_dummy);
    
    $.state.conditionDependentObjects.forEach(obj => {
        obj.send("exp_updateConditions", $.state.conditions);
    });
}

// --------- Initialize conditions from variables ----------

function getConditions (variables) {
    const repeatPerCombination = 2;
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