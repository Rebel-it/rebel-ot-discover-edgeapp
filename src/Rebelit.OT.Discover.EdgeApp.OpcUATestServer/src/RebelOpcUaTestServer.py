import asyncio
import math

from asyncua import Server, ua
from asyncua.server.user_managers import UserManager

#Simulate a realistic motor temperature profile.
def calculate_motor_temperature(counter: int) -> float:
    
    warmup = min(counter / 120.0, 1.0) * 4.0
    load_wave = 6.0 * math.sin(2.0 * math.pi * counter / 120.0)
    ripple = 1.2 * math.sin(2.0 * math.pi * counter / 17.0)

    phase = counter % 180
    if 130 <= phase < 138:
        spike = (phase - 130) * 1.2
    elif 138 <= phase < 146:
        spike = (146 - phase) * 1.2
    else:
        spike = 0.0

    cooldown = -3.0 if 165 <= phase < 175 else 0.0
    temp_value = max(45.0, min(110.0, 72.0 + warmup + load_wave + ripple + spike + cooldown))
    return float(round(temp_value, 2))

def calculate_machine_cleanliness(counter: int) -> float:
    base_cleanliness = 100.0
    # Faster dirt accumulation: 40 points lost over 2 minutes (120s)
    dirt_accumulation = min(counter / 120.0, 1.0) * 40.0
    # Smaller oscillation
    cleaning_cycle = 0.5 * math.sin(2.0 * math.pi * counter / 60.0)
    cleanliness_value = base_cleanliness - dirt_accumulation + cleaning_cycle
    cleanliness_value = min(100.0, cleanliness_value)  # Clamp to max 100
    cleanliness_value = max(55.0, cleanliness_value)   # Clamp to min 55
    return float(round(cleanliness_value, 2))


async def update_nodes_loop(
    uint_variable_16,
    uint_variable_32,
    uint_variable_64,
    int_variable_16,
    int_variable_32,
    int_variable_64,
    bool_variable_true,
    bool_variable_false,
    string_variable,
    float_variable,
    double_variable,
    motor_temperature,
    machine_cleanliness,

):
    counter = 0
    while True:
        await uint_variable_16.write_value(ua.Variant(counter % 65536, ua.VariantType.UInt16))

        await uint_variable_32.write_value(ua.Variant((counter * 1000) % (2**32), ua.VariantType.UInt32))
        await uint_variable_64.write_value(ua.Variant((12345678901234567890 + counter) % (2**64), ua.VariantType.UInt64))

        await int_variable_16.write_value(ua.Variant((counter % 2000) - 1000, ua.VariantType.Int16))
        await int_variable_32.write_value(ua.Variant((counter % 200000) - 100000, ua.VariantType.Int32))
        await int_variable_64.write_value(ua.Variant((counter % 2000000) - 1000000, ua.VariantType.Int64))

        toggle = (counter % 2) == 0
        await bool_variable_true.write_value(ua.Variant(toggle, ua.VariantType.Boolean))
        await bool_variable_false.write_value(ua.Variant(not toggle, ua.VariantType.Boolean))

        await string_variable.write_value(ua.Variant(f"Hello, OPC UA! Tick {counter}", ua.VariantType.String))
        await float_variable.write_value(ua.Variant(3.14 + ((counter % 100) / 100.0), ua.VariantType.Float))
        await double_variable.write_value(ua.Variant(3.141592653589793 + (counter / 1000.0), ua.VariantType.Double))

        temp_value = calculate_motor_temperature(counter)
        await motor_temperature.write_value(ua.Variant(temp_value, ua.VariantType.Float))
        cleanliness_value = calculate_machine_cleanliness(counter)
        await machine_cleanliness.write_value(ua.Variant(cleanliness_value, ua.VariantType.Float))

        counter += 1
        await asyncio.sleep(1)

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

    server.set_endpoint("opc.tcp://0.0.0.0:53530/rebelit/server/")
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
    uint_variable_32 = await myobj.add_variable(ua.NodeId("UInt32Variable", idx), "MyUInt32Variable", 123, ua.VariantType.UInt32)
    uint_variable_64 = await myobj.add_variable(ua.NodeId("UInt64Variable", idx), "MyUInt64Variable", 12345678901234567890, ua.VariantType.UInt64)

    # int variables
    int_variable_16 = await myobj.add_variable(ua.NodeId("Int16Variable", idx), "MyInt16Variable", 1234, ua.VariantType.Int16)
    int_variable_32 = await myobj.add_variable(ua.NodeId("Int32Variable", idx), "MyInt32Variable", 123, ua.VariantType.Int32)
    int_variable_64 = await myobj.add_variable(ua.NodeId("Int64Variable", idx), "MyInt64Variable", 1234567890123456789, ua.VariantType.Int64)

    # bool variable
    bool_variable_true = await myobj.add_variable(ua.NodeId("BoolVariableTrue", idx), "MyBoolVariableTrue", True, ua.VariantType.Boolean)
    bool_variable_false = await myobj.add_variable(ua.NodeId("BoolVariableFalse", idx), "MyBoolVariableFalse", False, ua.VariantType.Boolean)

    # string variable
    string_variable = await myobj.add_variable(ua.NodeId("StringVariable", idx), "MyStringVariable", "Hello, OPC UA!", ua.VariantType.String)

    # float variable
    float_variable = await myobj.add_variable(ua.NodeId("FloatVariable", idx), "MyFloatVariable", 3.14, ua.VariantType.Float)
    
    # double variable
    double_variable = await myobj.add_variable(ua.NodeId("DoubleVariable", idx), "MyDoubleVariable", 3.141592653589793, ua.VariantType.Double)  

    #temperature variables
    motor_temperature = await myobj.add_variable(ua.NodeId("MotorTemperature", idx), "MotorTemperature", 75.0, ua.VariantType.Float)

    #cleanliness variable
    machine_cleanliness = await myobj.add_variable(ua.NodeId("MachineCleanliness", idx), "MachineCleanliness", 99.0, ua.VariantType.Float)

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
    await double_variable.set_writable()

    await motor_temperature.set_writable()
    await machine_cleanliness.set_writable()
    print("✅ Server running at: opc.tcp://127.0.0.1:53530/rebelit/server/")

    async with server:
        await update_nodes_loop(
            uint_variable_16,
            uint_variable_32,
            uint_variable_64,
            int_variable_16,
            int_variable_32,
            int_variable_64,
            bool_variable_true,
            bool_variable_false,
            string_variable,
            float_variable,
            double_variable,
            motor_temperature,
            machine_cleanliness,
        )

if __name__ == "__main__":
    asyncio.run(main())