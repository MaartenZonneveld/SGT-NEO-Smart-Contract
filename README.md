<p align="center">
  <img src="https://safeguard-app.com/wp-content/themes/safeguard2018/assets/img/logo_uncompressed.png">
</p>
<H3 align="center">Safeguard Token NEO Smart Contract</H3>
<p align="center">Authors: Gertjan Leemans, Maarten Zonneveld & Zehna van den Berg</p>

Based on the official NEO Example ICO Template found at: https://github.com/neo-project/examples-csharp/tree/master/ICO_Template,
in combination with the NEO ICO Smart Contract by Thor found at: https://github.com/thortoken/neo-ico-smartcontract
<hr/>

## Smart Contract functions
### Deploy (Checks for Owner)

In neo-python prompt:

```neo-python
testinvoke {contract_hash} deploy []
```

### Check total supply and tokens sold in the crowdsale

In neo-python:

```neo-python
testinvoke {contract_hash} totalSupply []
testinvoke {contract_hash} crowdsaleTokensSold []
```

### Register for whitelist (Checks for Owner)

In neo-python:

```neo-python
testinvoke {contract_hash} crowdsaleRegister ['AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y']
testinvoke {contract_hash} crowdsaleRegistrationStatus ['AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y']
```

### Update personal caps (Checks for Owner)

In neo-python:

```neo-python
testinvoke {contract_hash} crowdsaleChangePersonalCap [1000]
testinvoke {contract_hash} crowdsalePersonalCap []
```

### Mint Tokens (Checks for Whitelist)

In neo-python:

```neo-python
testinvoke {contract_hash} mintTokens []
```

### Airdrop tokens - For privatesale (Checks for Owner & Whitelist)

In neo-python:

```neo-python
testinvoke {contract_hash} airdropTokens ['AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y', 1000]
```
