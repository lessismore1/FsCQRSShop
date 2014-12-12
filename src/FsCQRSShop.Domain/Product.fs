﻿namespace FsCQRSShop.Domain
open FsCQRSShop.Contract
open Commands
open Events
open Types

open State
open Railway


module Product =
    type ProductInfo = {Id: ProductId; Name: string; Price: int}
    type Product = 
    | Init
    | Created of ProductInfo

    let evolveOneProduct state event = 
        match state with
        | Init -> match event with
                  | ProductCreated(id, name, price) -> Success (Created {Id = id; Name = name; Price = price})
                  | _ -> stateTransitionFail state event
        | _ -> stateTransitionFail state event

    let evolveProduct = evolve evolveOneProduct

    let handleProduct deps pc = 
        let getState id = evolveProduct Init ((deps.readEvents id) |> (fun (_, e) -> e))
        let createProduct id name price (version,state) = 
            match state with 
            | Init -> Success (id, version, [ProductCreated(ProductId id, name, price)])
            | _ -> Failure (InvalidState "Product")
        match pc with
        | CreateProduct(ProductId id, name, price) -> 
            getState id >>= createProduct id name price
