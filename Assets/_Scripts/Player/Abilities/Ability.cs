using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Data;
using System;
using static UnityEngine.EventSystems.EventTrigger;

public class Ability : MonoBehaviour
{

    public GameObject abilityEffectPrefab;

    [SerializeField]
    private LayerMask enemyLayer;

    public string abilityName;
    public string abilityType;
    public float abilityCooldown;
    public float attackDamage;
    public float abilityRange;
    public float projectileSpeed;
    public int damageReduction;
    public float abilityDuration;
    public KeyCode abilityKey;

    private bool isOnCooldown = false;
    private string dbPath;

    void Start()
    {
        dbPath = "URI=file:" + Application.dataPath + "/Database.db";
        LoadAbilityStats();
        Debug.Log($"Loaded ability: {abilityName}");

        if (!string.IsNullOrEmpty(abilityName))
        {
            Initialize(abilityName);
        }
       
    }

    private void LoadAbilityStats()
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Abilities WHERE abilityName = @name";
                cmd.Parameters.Add(new SqliteParameter("@name", abilityName));

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        abilityType = reader["abilityType"].ToString();
                        abilityCooldown = Convert.ToSingle(reader["abilityCooldown"]);
                        attackDamage = Convert.ToSingle(reader["attackDamage"]);
                        abilityRange = Convert.ToSingle(reader["abilityRange"]);
                        projectileSpeed = Convert.ToSingle(reader["projectileSpeed"]);
                        damageReduction = Convert.ToInt32(reader["damageReduction"]);
                        abilityDuration = Convert.ToSingle(reader["abilityDuration"]);
                        abilityKey = (KeyCode)Enum.Parse(typeof(KeyCode), reader["abilityKeyCode"].ToString());

                        Debug.Log($"[Ability] Loaded: {abilityName} - Damage: {attackDamage}, Cooldown: {abilityCooldown}");
                    }
                }
            }
            conn.Close();
        }
    }

    public void Initialize(string abilityName)
    {
        AbilitiesDatabaseConn db = new AbilitiesDatabaseConn(abilityName);

        this.abilityName = abilityName;
        abilityType = db.GetStringValue("abilityType");
        abilityCooldown = db.GetAbilityCooldown();
        attackDamage = db.GetAbilityAttackDamage();
        abilityRange = db.GetFloatValue("abilityRange");
        projectileSpeed = db.GetFloatValue("projectileSpeed");
        damageReduction = db.GetIntValue("damageReduction");
        abilityDuration = db.GetFloatValue("abilityDuration");

        string keyCodeStr = db.GetStringValue("abilityKeyCode");
        abilityKey = (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeStr);

        Debug.Log($"Initialized {abilityName}: " +
                  $"Type={abilityType}, DMG={attackDamage}, CD={abilityCooldown}, Key={abilityKey}");
    }


    public void ModifyAbility(float extraDamage, float extraRange, float extraSpeed, float extraDuration)
    {
        attackDamage += (int)extraDamage;
        abilityRange += extraRange;
        projectileSpeed += extraSpeed;
        abilityDuration += extraDuration;

        Debug.Log($"{abilityName} modified! New Stats: Damage={attackDamage}, Range={abilityRange}, Speed={projectileSpeed}, Duration={abilityDuration}");
    }

    public void ResetToBaseStats()
    {
        AbilitiesDatabaseConn db = new AbilitiesDatabaseConn(abilityName);
        attackDamage = db.GetAbilityAttackDamage();
        abilityRange = db.GetFloatValue("abilityRange");
        projectileSpeed = db.GetFloatValue("projectileSpeed");
        abilityDuration = db.GetFloatValue("abilityDuration");

        Debug.Log($"{abilityName} reset! Damage={attackDamage}, Range={abilityRange}, Speed={projectileSpeed}, Duration={abilityDuration}");
    }


    void Update()
    {


    }
    private void InvokeAbilityMethod(string abilityName)
    {
        switch (abilityName.ToLower())
        {
            case "fire ring":
                Debug.Log("[DEBUG] Invoking ActivateFireRing()");
                Invoke(nameof(ActivateFireRing), 0f);
                break;
            case "wind slash":
                Debug.Log("[DEBUG] Invoking ActivateWindSlash()");
                Invoke(nameof(ActivateWindSlash), 0f);
                break;
            case "ultimate":
                Debug.Log("[DEBUG] Invoking ActivateUltimate()");
                Invoke(nameof(ActivateUltimate), 0f);
                break;
            default:
                Debug.LogWarning($"[DEBUG] No matching method found for abilityName: {abilityName}");
                break;
        }
    }
    public IEnumerator UseAbility()
    {
        Debug.Log($"[DEBUG] Ability {abilityName} from object: {gameObject.name}, tag: {gameObject.tag}, scene: {gameObject.scene.name}");

        if (isOnCooldown)
        {
            Debug.Log("[DEBUG] Ability is on cooldown, exiting.");
            yield break;
        }

        Debug.Log($"[DEBUG] Cooldown clear. Attempting to invoke {abilityName}");

       
        InvokeAbilityMethod(abilityName);

        isOnCooldown = true;

        Debug.Log($"[DEBUG] Starting cooldown for {abilityCooldown} seconds");

        yield return new WaitForSeconds(abilityCooldown);

        isOnCooldown = false;

        Debug.Log("[DEBUG] Cooldown ended.");
    }

    public void ActivateFireRing()
    {
        Debug.Log($"Activating {abilityName}");

        
        if (abilityEffectPrefab != null)
        {
            Instantiate(abilityEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, abilityRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<Health>().TakeDamageFromAbilities(attackDamage);
        }
    }


    private void ActivateWindSlash()
    {
        

        Debug.Log($"[WindSlash] Casting at speed {projectileSpeed} with range {abilityRange}");

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;

        GameObject proj = Instantiate(
            abilityEffectPrefab,
            transform.position,
            Quaternion.identity);

        

        Projectile projectile = proj.GetComponent<Projectile>();
       
        projectile.speed = projectileSpeed;
        projectile.damage = attackDamage;
        projectile.range = abilityRange;
        projectile.enemyLayer = enemyLayer;
        projectile.SetDirection(direction);
    }

    private void ActivateStoneArmor()
    {
        PlayerStats playerStats = GetComponent<PlayerStats>();
        
        Debug.Log("Stone Armor activated: Damage reduced!");
    }

    private void ActivateUltimate()
    {
        
        Debug.Log("Ultimate activated: Massive damage!");
    }

    void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, abilityRange);
    }
}
