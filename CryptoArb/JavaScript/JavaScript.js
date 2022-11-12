﻿var last;
var bonds;
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
	profit: 0,
	volume: 0,
	exchangeId: 1,
}


document.getElementById("filtersOptions").hidden = true;
document.getElementById("botOptions").hidden = true;


const table = document.getElementById('bondsTable');
const cols = table.querySelectorAll('th');

// Loop over them
SetWidthInPercentage();
Resize();

function SetWidthInPercentage() {
	let windowSizeX = table.clientWidth;
	var element = document.getElementById("Symbol");
	element.style.width = `${element.clientWidth / windowSizeX * 100}%`;
	var element = document.getElementById("ExchangeId");
	element.style.width = `${element.clientWidth / windowSizeX * 100}%`;
	var element = document.getElementById("Volume");
	element.style.width = `${element.clientWidth / windowSizeX * 100}%`;
	var element = document.getElementById("Profit");
	element.style.width = `${element.clientWidth / windowSizeX * 100}%`;
}
function Resize() {
	for (var i = 0; i < cols.length - 1; i++) {

		// Create a resizer element
		const resizer = document.createElement('div');
		resizer.classList.add('resizer');

		// Set the height
		resizer.style.height = `${table.offsetHeight}px`;

		// Add a resizer element to the column
		cols[i].appendChild(resizer);

		// Will be implemented in the next section
		createResizableColumn(cols[i], resizer);
	}
}

function createResizableColumn(col, resizer) {
	// Track the current position of mouse
	let x = 0;
	let w = 0;


	const mouseDownHandler = function (e) {
		// Get the current mouse position
		x = e.clientX;

		// Calculate the current width of column
		const styles = window.getComputedStyle(col);
		w = parseInt(styles.width, 10);

		console.log(w);
		// Attach listeners for document's events
		document.addEventListener('mousemove', mouseMoveHandler);
		document.addEventListener('mouseup', mouseUpHandler);
	};

	const mouseMoveHandler = function (e) {
		// Determine how far the mouse has been moved
		const dx = e.clientX - x;

		// Update the width of column
		let windowSizeX = table.clientWidth;
		console.log(windowSizeX);
		col.style.width = `${(w + dx) / windowSizeX * 100}%`;
	};

	// When user releases the mouse, remove the existing event listeners
	const mouseUpHandler = function () {
		document.removeEventListener('mousemove', mouseMoveHandler);
		document.removeEventListener('mouseup', mouseUpHandler);
	};

	resizer.addEventListener('mousedown', mouseDownHandler);
};

// Получение всех пользователей
async function GetBinancePrices() {
	const binancePricesResponse = await fetch("/api/intraexchangebond?" + new URLSearchParams({
		exchangeId: filters.exchangeId.toString(),
		volume: filters.volume.toString(),
		profit: filters.profit.toString(),
	}, {
		method: "GET",
		headers: { "Accept": "application/json" },
	}));

	if (binancePricesResponse.ok) {
		const binance = await binancePricesResponse.json();
		bonds = binance;

	}
}

function SubscribeToEvent() {
	document.getElementById("symbolbutton").addEventListener("click", SymbolSort);
	document.getElementById("exchangeIdbutton").addEventListener("click", ExchangeIdSort);
	document.getElementById("volumebutton").addEventListener("click", VolumeSort);
	document.getElementById("profitbutton").addEventListener("click", ProfitSort);

	document.getElementById("applyButton").addEventListener("click", SetFilters);
}

function SetFilters() {
	let volumeInput = document.getElementById("volumeInput");
	let profitInput = document.getElementById("profitInput");

	filters.volume = parseFloat(volumeInput.value);
	filters.profit = parseFloat(profitInput.value);
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

async function SortBonds() {

	if (sortParams.symbol != 0) {
		bonds.sort((p1, p2) => sortParams.symbol * (p1.uSymbol > p2.uSymbol ? 1 : -1));
	}
	else if (sortParams.baseAsset != 0) {
		bonds.sort((p1, p2) => sortParams.baseAsset * (p1.baseAsset > p2.baseAsset ? 1 : -1));
	}
	else if (sortParams.quoteAsset != 0) {
		bonds.sort((p1, p2) => sortParams.quoteAsset * (p1.quoteAsset > p2.quoteAsset ? 1 : -1));
	}
	else if (sortParams.profit != 0) {
		bonds.sort((p1, p2) => sortParams.profit * (p1.profit > p2.profit ? 1 : -1));
	}
	else if (sortParams.volumeUSD != 0) {
		bonds.sort((p1, p2) => sortParams.volumeUSD * (p1.volumeUSD > p2.volumeUSD ? 1 : -1));
	}
	else if (sortParams.exchangeId != 0) {
		bonds.sort((p1, p2) => {
			let k = sortParams.exchangeId;
			if (p1.exchangeId > p2.exchangeId) { return k; }
			if (p1.exchangeId < p2.exchangeId) { return -K; }
			if (p1.id > p2.id) { return k; }
			if (p1.id < p2.id) { return -k; }
			return 0;
		});
	}
	else {

		bonds.sort((p1, p2) => p1.id > p2.id ? 1 : -1);
	}
}

async function UpdateTable() {
	const rows = document.getElementById("bin");
	rows.innerHTML = "";
	SortBonds();
	bonds.forEach(bond => rows.append(row(bond)));
}
//Call every 5 seconds get methods
function update() {
	//GetKucoinPrices();
	if (bonds != null)
		UpdateTable();
	GetBinancePrices();

	setTimeout(update, 3000);
}

function row(bond) {

	let tbody = document.getElementById("bin");
	let id = tbody.childElementCount;

	const tr = document.createElement("tr");
	tr.setAttribute("data-rowid", bond.uSymbol);


	const idTd = document.createElement("td");
	idTd.append(id);
	tr.append(idTd);

	const USymbolTd = document.createElement("td");
	USymbolTd.append(bond.uSymbol);
	tr.append(USymbolTd);

	const exchangeIDTd = document.createElement("td");
	exchangeIDTd.append(bond.exchangeId);
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
update();
