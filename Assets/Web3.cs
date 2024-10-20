using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using Thirdweb.Unity;
using Thirdweb.Unity.Examples;
using Thirdweb.Editor;
using Thirdweb.Pay;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class Web3 : MonoBehaviour
{
    private ulong ActiveChainId = 777777;
    private ThirdwebChainData _chainDetails;

    [SerializeField]
    GameObject confetti;
    [SerializeField]
    GameObject confettiPos;

    [SerializeField]
    GameObject activateOnWin;
    [SerializeField] GameObject slotsLoose;
    [SerializeField] GameObject slotsWin;

    [SerializeField] GameObject login;
    [SerializeField] TMP_InputField email;
    [SerializeField] Button confirmLogin;

    PrivateKeyWallet walletCasino;
    PrivateKeyWallet walletUser;

    string emailLogin = "sefef53383@avzong.com";

    //wallet2 (Casino) 0xa3E2Be5a7eD0Cd6836280a1Fb45869103497BEaf
    // "from": "0xb4175e37b812a43ffb4f1fde5e380085f6e80214",
    //"to": "0xa3e2be5a7ed0cd6836280a1fb45869103497beaf",
    string addressUser;
    string addressCasino;

    public Button btnGamble;

    public Button btnTest;

    public TMP_Text tmWBalanceUser;
    public TMP_Text tmWBalanceCasino;
    public TMP_Text tmWalletUser;
    public TMP_Text tmWalletCasino;
    public TMP_Text tmWBalanceGame20;
    public TMP_Text tmWon;
    public TMP_Dropdown tmToken;


    static string tokenNative = "";
    static string game20 = "0x6C6AbF11FB431Ab78951F7E872a36d6b4Dc059af";
    string currentToken = tokenNative;

    private async void Start()
    {
        tmWBalanceUser.text = "";
        tmWBalanceCasino.text = "";
        tmWalletUser.text = "";
        tmWalletCasino.text = "";
        tmWBalanceGame20.text = "";
        tmWon.text = "";
        activateOnWin.SetActive(false);
        slotsLoose.SetActive(false);

        login.SetActive(true);
        confirmLogin.onClick.AddListener(async () =>
        {
            emailLogin = email.text;
            Debug.Log(emailLogin);
            login.SetActive(false);
            await GetWallets();
        });

        try
        {
            Debug.Log(ActiveChainId);
            _chainDetails = await Utils.GetChainMetadata(client: ThirdwebManager.Instance.Client, chainId: ActiveChainId);
        }
        catch
        {
            _chainDetails = new ThirdwebChainData()
            {
                NativeCurrency = new ThirdwebChainNativeCurrency()
                {
                    Decimals = 18,
                    Name = "ETH",
                    Symbol = "ETH"
                }
            };
        }
        

        //await GetWallets();

        tmToken.onValueChanged.AddListener(async (int index) =>
        {
            switch (index)
            {
                case 0:
                    currentToken = tokenNative;
                    break;
                default:
                    currentToken = game20;
                    break;
            }

            await UpdateBalances();
        });

        btnGamble.onClick.AddListener(async () =>
        {
            tmWon.text = "";
            activateOnWin.SetActive(false);
            slotsLoose.SetActive(false);


            tmWon.text = "Gamble 1";

            //user to casino
            var res = await walletUser.Transfer(ActiveChainId, addressCasino, System.Numerics.BigInteger.Pow(10, 18));
            Debug.Log($"Transfer: {res}");

            //do random
            var r = Random.Range(0, 2);

            System.Numerics.BigInteger winAmount = 0;
            if (r == 1)
            {
                DoConfetti();
                await walletCasino.Transfer(ActiveChainId, addressUser, 2 * System.Numerics.BigInteger.Pow(10, 18));

                tmWon.text = "Won 2!";
                activateOnWin.SetActive(true);
            }
            else
            {
                tmWon.text = "Lost 1";
                slotsLoose.SetActive(true);
            }

            await UpdateBalances();
        });

 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DoConfetti()
    {
        var c = Instantiate(confetti, confettiPos.transform.position, UnityEngine.Quaternion.identity) as GameObject;
        //scale to 100
        c.transform.localScale = new Vector3(100, 100, 100);
    }

    async Task UpdateBalances(){

        var balance = System.Numerics.BigInteger.Zero;
        var balanceStr = "";

        if (currentToken == tokenNative)
        {
            balance = await walletUser.GetBalance(chainId: ActiveChainId);
            balanceStr = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 2, addCommas: true);
            tmWBalanceUser.text = balanceStr + _chainDetails.NativeCurrency.Symbol;

        }
        else
        {
            var dropErc20Contract = await ThirdwebManager.Instance.GetContract(address: currentToken, chainId: ActiveChainId);
            var symbol = await dropErc20Contract.ERC20_Symbol();
            balance = await walletUser.GetBalance(chainId: ActiveChainId, currentToken);
            balanceStr = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 2, addCommas: true);
            //tmWBalanceGame20.text = balanceStr + symbol;
            tmWBalanceUser.text = balanceStr + symbol;
        }
        
        balance = await walletCasino.GetBalance(chainId: ActiveChainId);
        balanceStr = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 2, addCommas: true);
        tmWBalanceCasino.text = balanceStr + _chainDetails.NativeCurrency.Symbol;
    }

    async Task GetWallets(){
        var inAppWalletOptions = new InAppWalletOptions(email: emailLogin);
        var options = new WalletOptions(provider: WalletProvider.InAppWallet, chainId: ActiveChainId, inAppWalletOptions: inAppWalletOptions);
        var uw = await ThirdwebManager.Instance.ConnectWallet(options);
        walletUser = uw as PrivateKeyWallet;
        addressUser = await walletUser.GetAddress();
        tmWalletUser.text = addressUser;

        var client = walletUser.Client;//ThirdwebClient.Create("yourClientId");
        var privateKeyHex = "0x9aad452b463ded289bf97efe880b43b09d22bc60d80cfc8213362c4611ef587a"; // Should be securely stored and accessed
        walletCasino = await PrivateKeyWallet.Create(client, privateKeyHex);
        addressCasino = await walletCasino.GetAddress();
        tmWalletCasino.text = addressCasino;

        await UpdateBalances();
        await FundUser();

        return;
    }

    async Task FundUser(){
        var minBal = 10 * System.Numerics.BigInteger.Pow(10, 18);

        var balanceWei = await walletUser.GetBalance(chainId: ActiveChainId);
        if (balanceWei > minBal){
            return;
        }

        tmWon.text = "Welcome tokens 10!";


        //casino to user
        var res = await walletCasino.Transfer(ActiveChainId, addressUser, minBal);
        Debug.Log($"Transfer: {res}");

        await UpdateBalances();
    }
}
