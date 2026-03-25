import asyncio
from http import server
from asyncua import Server, ua
from asyncua.server.user_managers import UserManager

async def main():
    server = Server()

    await server.init()

    users_db = {
        "user1": "password1",
    }

    class MyUserManager(UserManager):
        async def user_manager(self, isession, username, password):
            return username in users_db and users_db[username] == password


    server.set_security_IDs(["Username"])
    server.user_manager = MyUserManager()

    server.set_endpoint("opc.tcp://127.0.0.1:53530/rebelit/server/")
    server.set_server_name("Rebel OPC UA Test Server")

    server.set_security_policy([ua.SecurityPolicyType.NoSecurity, ua.SecurityPolicyType.Basic256Sha256_Sign ,ua.SecurityPolicyType.Basic256Sha256_SignAndEncrypt])

    # Create namespace
    uri = "http://rebelit.nl"
    idx = await server.register_namespace(uri)

    # Create object
    objects = server.nodes.objects
    myobj = await objects.add_object(idx, "VariableTestObject")

    # Uint variables
    uint_variable_16 = await myobj.add_variable(ua.NodeId("UInt16Variable", idx), "MyUInt16Variable", 65535, ua.VariantType.UInt16)
    uint_variable_32 = await myobj.add_variable(ua.NodeId("UInt32Variable", idx), "MyUIntVariable", 123, ua.VariantType.UInt32)
    uint_variable_64 = await myobj.add_variable(ua.NodeId("UInt64Variable", idx), "MyUInt64Variable", 12345678901234567890, ua.VariantType.UInt64)

    # int variables
    int_variable_16 = await myobj.add_variable(ua.NodeId("Int16Variable", idx), "MyInt16Variable", 1234, ua.VariantType.Int16)
    int_variable_32 = await myobj.add_variable(ua.NodeId("Int32Variable", idx), "MyIntVariable", 123, ua.VariantType.Int32)
    int_variable_64 = await myobj.add_variable(ua.NodeId("Int64Variable", idx), "MyInt64Variable", 1234567890123456789, ua.VariantType.Int64)

    # bool variable
    bool_variable_true = await myobj.add_variable(ua.NodeId("BoolVariableTrue", idx), "MyBoolVariable", True, ua.VariantType.Boolean)
    bool_variable_false = await myobj.add_variable(ua.NodeId("BoolVariableFalse", idx), "MyBoolVariableFalse", False, ua.VariantType.Boolean)

    # string variable
    string_variable = await myobj.add_variable(ua.NodeId("StringVariable", idx), "MyStringVariable", "Hello, OPC UA!", ua.VariantType.String)

    # float variable
    float_variable = await myobj.add_variable(ua.NodeId("FloatVariable", idx), "MyFloatVariable", 3.14, ua.VariantType.Float)

    await uint_variable_16.set_writable()
    await uint_variable_32.set_writable()
    await uint_variable_64.set_writable()
    await int_variable_16.set_writable()
    await int_variable_32.set_writable()
    await int_variable_64.set_writable()
    await bool_variable_true.set_writable()
    await bool_variable_false.set_writable()
    await string_variable.set_writable()
    await float_variable.set_writable()

    print("✅ Server running at: opc.tcp://127.0.0.1:53530/rebelit/server/")

    async with server:
        while True:
            await asyncio.sleep(1)

if __name__ == "__main__":
    asyncio.run(main())