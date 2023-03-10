var last;
var bonds = [];
var infoTimeoutID;
var sortParams = {
	symbol: 0,
	baseAsset: 0,
	quoteAsset: 0,
	profit: 0,
	volumeUSD: 0,
	exchangeId: 1,
}
var filters = {
	showVolume: true,
	showProfit: true,
	showBaseAsset: true,
	showQuoteAsset: true,
	showSymbol: true,
	showBond: true,
	showRows: 100,
	profit: 0,
	volume: 0,
	exchangeId: 1,
}

async function GetBinancePrices() {
	const binancePricesResponse = await fetch("/api/intraexchangebond?" + new URLSearchParams({
		exchangeId: filters.exchangeId.toString(),
		volume: 0,
		profit: 0,
	}, {
		method: "GET",
		headers: { "Accept": "application/json" },
	}));

	if (binancePricesResponse.ok) {
		const binance = await binancePricesResponse.json();
		bonds = binance;
	} else {
		console.error("bonds")
	}
}

async function Form(apiId, key) {
	const rawResponse = await fetch("/api/data", {
		method: 'POST',
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify({ "ApiID": apiId, "Key": key })
	});
	try {
		const content = await rawResponse.json();
	} catch (error) {
		console.error(error);
		content = "";
	}
	if (rawResponse.status == 200) {
		Info("Success ✅", 5000);
	} else {
		Info("Faild To Start ❌", 5000)
	}
	//console.log(content);
	//return content.isSuccess;
}
function Info(text, time) {
	info = document.getElementById("info");
	if (infoTimeoutID != null) {
		clearTimeout(infoTimeoutID);
	}
	info.parentElement.hidden = false;
	info.innerHTML = text;
	infoTimeoutID = setTimeout(() => { info.parentElement.hidden = true; }, time);
}

function SubscribeToEvent() {
	document.getElementById("Symbol").addEventListener("click", SymbolSort);
	document.getElementById("ExchangeId").addEventListener("click", ExchangeIdSort);
	document.getElementById("Volume").addEventListener("click", VolumeSort);
	document.getElementById("Profit").addEventListener("click", ProfitSort);
}

function SetFilters() {
	let volumeInput = document.getElementById("volumeInput");
	let profitInput = document.getElementById("profitInput");
	let showRows = document.getElementById("showRows");

	filters.showRows = ValidateApply(showRows);
	filters.volume = ValidateApply(volumeInput);
	filters.profit = ValidateApply(profitInput);

	function ValidateApply(inputField) {
		if (isNaN(inputField.valueAsNumber)) {
			return 0;
		}
		return inputField.valueAsNumber;
	}
}


// --------------------------SORTS-----------------------------
function SymbolSort() {
	let symbolSort = sortParams.symbol;
	clearSorts();

	if (symbolSort == 1) {
		symbolSort = -1;
	}
	else
		symbolSort++;

	let scale = `scale(1,${symbolSort} )`;
	document.getElementById("symbolsort").style.transform = scale;

	sortParams.symbol = symbolSort;

	UpdateTable();
}

function ExchangeIdSort() {
	let exchangeIdSort = sortParams.exchangeId;
	clearSorts();

	if (exchangeIdSort == 1) {
		exchangeIdSort = -1;
	}
	else
		exchangeIdSort++;

	let scale = `scale(1,${exchangeIdSort} )`;
	document.getElementById("exchangeIdsort").style.transform = scale;

	sortParams.exchangeId = exchangeIdSort;

	UpdateTable();
}

function VolumeSort() {
	let volumeUSDSort = sortParams.volumeUSD;
	clearSorts();

	if (volumeUSDSort == 1) {
		volumeUSDSort = -1;
	}
	else
		volumeUSDSort++;

	let scale = `scale(1,${volumeUSDSort} )`;
	document.getElementById("volumesort").style.transform = scale;

	sortParams.volumeUSD = volumeUSDSort;

	UpdateTable();
}

function ProfitSort() {
	let profitSort = sortParams.profit;
	clearSorts();

	if (profitSort == 1) {
		profitSort = -1;
	}
	else
		profitSort++;

	let scale = `scale(1,${profitSort} )`;
	document.getElementById("profitsort").style.transform = scale;

	sortParams.profit = profitSort;

	UpdateTable();
}

function clearSorts() {
	sortParams.symbol = 0;
	sortParams.profit = 0;
	sortParams.quoteAsset = 0;
	sortParams.exchangeId = 0;
	sortParams.baseAsset = 0;
	sortParams.volumeUSD = 0;
	let scale = "scale(0)"
	document.getElementById("symbolsort").style.transform = scale;
	document.getElementById("exchangeIdsort").style.transform = scale;
	document.getElementById("volumesort").style.transform = scale;
	document.getElementById("profitsort").style.transform = scale;
}

function SortBonds(array) {

	if (sortParams.symbol != 0) {
		array.sort((p1, p2) => sortParams.symbol * (p1.uSymbol > p2.uSymbol ? 1 : -1));
	}
	else if (sortParams.baseAsset != 0) {
		array.sort((p1, p2) => sortParams.baseAsset * (p1.baseAsset > p2.baseAsset ? 1 : -1));
	}
	else if (sortParams.quoteAsset != 0) {
		array.sort((p1, p2) => sortParams.quoteAsset * (p1.quoteAsset > p2.quoteAsset ? 1 : -1));
	}
	else if (sortParams.profit != 0) {
		array.sort((p1, p2) => sortParams.profit * (p1.profit > p2.profit ? 1 : -1));
	}
	else if (sortParams.volumeUSD != 0) {
		array.sort((p1, p2) => sortParams.volumeUSD * (p1.volumeUSD > p2.volumeUSD ? 1 : -1));
	}
	else if (sortParams.exchangeId != 0) {
		array.sort((p1, p2) => {
			let k = sortParams.exchangeId;
			if (p1.exchangeId > p2.exchangeId) { return k; }
			if (p1.exchangeId < p2.exchangeId) { return -K; }
			if (p1.id > p2.id) { return k; }
			if (p1.id < p2.id) { return -k; }
			return 0;
		});
	}
	else {
		array.sort((p1, p2) => p1.id > p2.id ? 1 : -1);
	}
}

function Filter() {
	return bonds.filter(bond => bond.profit >= filters.profit && bond.volumeUSD >= filters.volume);
}

async function UpdateTable() {
	const rows = document.getElementById("bin");
	rows.innerHTML = "";
	var array = Filter();
	SortBonds(array);

	for (var i = 0; i < filters.showRows; i++) {
		let bond = array[i];
		rows.append(row(bond));
	}
}
//Call every 5 seconds get methods
function Update() {
	//GetKucoinPrices();

	GetBinancePrices();
	if (bonds.length > 0)
		UpdateTable();
	else {
		Info("Loading...", 5000);
	}

	setTimeout(Update, 5000);
}

setTimeout(Info, 136070, "Found ⏱", 5000);
setTimeout(Info, 246070, "Finish +0.7556% 📈", 5000);

function row(bond) {

	let tbody = document.getElementById("bin");
	let id = tbody.childElementCount;

	const tr = document.createElement("tr");
	tr.setAttribute("data-rowid", id);

	let x = bonds;
	const idTd = document.createElement("td");
	idTd.append(id);
	tr.append(idTd);

	const USymbolTd = document.createElement("td");
	USymbolTd.append(bond.uSymbol);
	tr.append(USymbolTd);

	const exchangeIDTd = document.createElement("td");
	exchangeIDTd.append(bond.exchangeId == 1 ? "Binance": 1 );
	tr.append(exchangeIDTd);

	const VolumeUSDTd = document.createElement("td");
	VolumeUSDTd.append(bond.volumeUSD);
	tr.append(VolumeUSDTd);

	const ProfitTd = document.createElement("td");
	ProfitTd.append(bond.profit);
	tr.append(ProfitTd);

	const BondTd = document.createElement("td");
	BondTd.append(bond.sequencing.join("->"));
	tr.append(BondTd);

	return tr;
}
// сброс значений формы

SubscribeToEvent();
Update();