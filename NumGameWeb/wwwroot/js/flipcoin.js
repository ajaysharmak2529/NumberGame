const walletBalanceElement = document.getElementById("Wallet");
const headButton = document.getElementById("HeadsBtn");
const tailButton = document.getElementById("TailsBtn");
const confirmButton = document.getElementById("Confirmclick");
const clearButton = document.getElementById("Cancelclick");
const betelement = document.getElementById("TotalBet");
const storedWalletBalance = localStorage.getItem('walletBalance');
const priceButtons = document.getElementById('pricebutons');
let coin = document.querySelector(".coin");
let betOnHeads = 0;
let betOnTails = 0;
let totalBet = 0;
let isConfirm = false;

let coinData = [];

headButton.addEventListener('click', () => installBet('Heads'));
tailButton.addEventListener('click', () => installBet('Tails'));

clearButton.addEventListener("click", () => {
    if (!isConfirm) {        
        clearBet();
    }
});


function clearBet() {
    isConfirm = false;
    headButton.disabled = false;
    tailButton.disabled = false;
    coinData = [];
    const basgeButton = document.querySelectorAll("#ButonsCoins button");
    basgeButton.forEach((element) => {
        let childelement = element.querySelector("span");
        if (childelement) {
            element.removeChild(childelement);
        }
    });
    totalBet = 0;
    betelement.innerText = (totalBet);
}

function ShowAlert(type, message) {
    const modal = document.getElementById('notificationModal');
    const icon = document.getElementById('notificationIcon');
    const messageElement = document.getElementById('notificationMessage');

    if (type === 'success') {
        icon.className = 'fas fa-check-circle text-success';
    } else if (type === 'failure') {
        icon.className = 'fas fa-times-circle text-danger';
    }

    messageElement.textContent = message;
    $(modal).modal('show'); // Use jQuery to show the modal
}
function installBet(coinSide) {
    const storedWalletBalance = localStorage.getItem('walletBalance');
    const selectedAmountInput = priceButtons.querySelector('input[type="radio"]:checked');
    const selectedAmount = selectedAmountInput.checked ? parseInt(selectedAmountInput.value) : 0;

    if (storedWalletBalance !== null && isConfirm == false) {
        
        const walletBalance = parseInt(storedWalletBalance);
        totalBet += selectedAmount;
        if (!isConfirm) {
            if (walletBalance >= totalBet) {
                if (coinSide === "Heads") {
                    tailButton.disabled = true;
                    const bettingInfo = coinData.find(info => info.coin_type === "head");
                    const badge = document.querySelector('#HeadsBtn span');
                    if (bettingInfo) {
                        bettingInfo.amount += selectedAmount;
                        badge.innerHTML = `${totalBet}₹`
                    } else {
                        const bettingInfo = {
                            coin_type: "head",
                            amount: selectedAmount
                        };
                        coinData.push(bettingInfo);
                        addBadgeToButton(headButton, totalBet);
                    }
                }
                else {
                    headButton.disabled = true;
                    const bettingInfo = coinData.find(info => info.coin_type === "tail");
                    const badge = document.querySelector('#TailsBtn span');

                    if (bettingInfo) {
                        bettingInfo.amount += selectedAmount;
                        badge.innerHTML = `${totalBet}₹`
                    } else {
                        const bettingInfo = {
                            coin_type: "tail",
                            amount: selectedAmount
                        };
                        coinData.push(bettingInfo);
                        addBadgeToButton(tailButton, totalBet);
                    }
                }
            } else {
                ShowAlert("failure", "Your bet reached to your wallet balance please recharge your wallet to continoue!!");
                totalBet -= selectedAmount;
            }
        }
        
        betelement.innerText = totalBet;        
    }


}
function disableButtons() {
    document.querySelectorAll("button").forEach((button) => {
        button.disabled = true;
    });
    checkboxes.forEach((check) => { check.disabled = true })
}

// Function to enable all buttons
function enableButtons() {
    document.querySelectorAll("button").forEach((button) => {
        button.disabled = false;
    });
    checkboxes.forEach((x) => x.disabled = false);
}
function addBadgeToButton(targetButton, ammount) {

    const badgeContainer = document.createElement('span');
    badgeContainer.classList.add('position-absolute', 'top-0', 'start-50', 'translate-middle', 'badge', 'rounded-pill', 'bg-success');
    badgeContainer.innerHTML = `${ammount}₹`;
    targetButton.appendChild(badgeContainer);
}

function startSpinning() {
    coin.innerHTML = '<div class="heads"><img src="/heads.png"></div><div class="tails"><img src="/tails.png"></div>';
    coin.classList.add("coin-spin");
}
function showResult(result) {
    let coinresultElement = document.getElementById("CoinResult");
    if (result === "HEAD") {  
        coin.classList.remove("coin-spin");
        coin.innerHTML = '<div><img src="/heads.png"></div>';
    } else {
        coin.classList.remove("coin-spin");
        coin.innerHTML = '<div><img src="/tails.png"></div>';
    }
    coinresultElement.innerText = result;
}

// Connect to the SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/updateHub")
    .build();

connection.on("UpdateTimer", (message) => {
    var timerElement = document.getElementById("timer");
    timerElement.innerHTML = `${message}`;
});
connection.on("DisableButtons", () => {
    startSpinning();
    disableButtons();
});
connection.on("EnableButtons", () => {
    enableButtons();
});

connection.on("updateCoinResult", (result) => {
    if (result != null) {
        totalBet = 0;
        showResult(result);
        clearBet();
    }
});
connection.on("NotifyCoinWinner", (result) => {
    if (result != null) {
        let numberFeild = document.getElementById("WinningNumber");
        let priceFeild = document.getElementById("WinningPrice");

        numberFeild.innerText = result.coinSide;
        priceFeild.innerText = result.amount;
        $('#WinnerModal').modal('show');
        let walletElement = document.getElementById("Wallet");
        walletElement.innerText = `${result.userWallte} ₹`;
        showResult(result);
    }
});
confirmButton.addEventListener("click", () => {
    if (!isConfirm) {
        if (totalBet > 0) {
            var betJson = JSON.stringify(coinData);
            connection.invoke("ConfirmCoinBet", betJson).then((x) => {
                if (x.status == true) {
                    let walletElement = document.getElementById("Wallet");
                    walletElement.innerText = `${x.wallet_balance} ₹`;
                    localStorage.setItem('walletBalance', x.wallet_balance);
                    ShowAlert("success", "Your bet SuccessFully saved")
                    isConfirm = true;
                } else {
                    ShowAlert("failure", x.message);
                    isConfirm = false;
                }

            }).catch(function (error) {
                console.error(error);
                isConfirm = false;
                ShowAlert("failure", "We are Facing Some Problem please try again after some time!")
            });
        }
    }
});

connection.start()
    .catch(err => console.error(err));
