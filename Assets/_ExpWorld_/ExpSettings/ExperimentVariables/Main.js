const trialsCountForEachUniqueCondition = 2;
const within_subjects_variables = [
    { name: "color", values: ["W", "B"], isRandom: false },
    { name: "size", values: ["10", "20", "30"], isRandom: true },
];
const between_subjects_variables = [
    { name: "method", values: ["new", "old"], isRandom: false },
];
